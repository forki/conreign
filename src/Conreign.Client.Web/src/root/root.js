import { combineReducers } from 'redux';
import { combineEpics } from 'redux-observable';
import { get, isArray } from 'lodash';
import serializeError from 'serialize-error';

import Rx from './../rx';
import {
  AsyncOperationState,
  createEventAction,
} from '../framework';
import errors from './../errors';
import notifications from './../notifications';
import auth from './../auth';
import home from './../home';
import room from './../room';

const LISTEN_FOR_SERVER_EVENTS = 'LISTEN_FOR_SERVER_EVENTS';
const EXECUTE_ROUTE_ACTIONS = 'EXECUTE_ROUTE_ACTIONS';
const BEGIN_ROUTE_TRANSACTION = 'BEGIN_ROUTE_TRANSACTION';
const END_ROUTE_TRANSACTION = 'END_ROUTE_TRANSACTION';
const REPORT_RENDERING_ERROR = 'REPORT_RENDERING_ERROR';

export function listenForServerEvents() {
  return { type: LISTEN_FOR_SERVER_EVENTS };
}

export function beginRouteTransaction(payload) {
  return {
    type: BEGIN_ROUTE_TRANSACTION,
    payload,
  };
}

function endRouteTransaction() {
  return { type: END_ROUTE_TRANSACTION };
}

export function reportRenderingError(error) {
  return {
    type: REPORT_RENDERING_ERROR,
    payload: serializeError(error),
    error: true,
  };
}

export function executeRouteActions(actions) {
  return {
    type: EXECUTE_ROUTE_ACTIONS,
    payload: actions,
  };
}

const INITIAL_OPERATIONS_STATE = {
  routePending: 0,
  totalPending: 0,
};

function positiveOrZero(value) {
  return value > 0 ? value : 0;
}

export function isRouteLoadingAction(action) {
  return get(action, 'meta.$route');
}

function operationsReducer(state = INITIAL_OPERATIONS_STATE, action) {
  switch (action.type) {
    case BEGIN_ROUTE_TRANSACTION:
      return {
        ...state,
        routePending: state.routePending + 1,
      };
    case END_ROUTE_TRANSACTION:
      return {
        ...state,
        routePending: positiveOrZero(state.routePending - 1),
      };
    default: {
      const asyncState = get(action, 'meta.$async.state');
      const isRouteAction = isRouteLoadingAction(action);
      switch (asyncState) {
        case AsyncOperationState.Pending:
          return {
            ...state,
            routePending: isRouteAction
              ? state.routePending + 1
              : state.routePending,
            totalPending: state.totalPending + 1,
          };
        case AsyncOperationState.Failed:
        case AsyncOperationState.Succeeded:
          return {
            ...state,
            routePending: isRouteAction
              ? positiveOrZero(state.routePending - 1)
              : state.routePending,
            totalPending: positiveOrZero(state.totalPending - 1),
          };
        default:
          return state;
      }
    }
  }
}

const reducer = combineReducers({
  operations: operationsReducer,
  [auth.reducer.$key]: auth.reducer,
  [errors.reducer.$key]: errors.reducer,
  [notifications.reducer.$key]: notifications.reducer,
  [room.reducer.$key]: room.reducer,
});

function createEpic(container) {
  const { apiClient } = container;

  function listenForServerEventsEpic(action$) {
    return action$
      .ofType(LISTEN_FOR_SERVER_EVENTS)
      .mergeMap(() => apiClient.events)
      .map(createEventAction);
  }

  function mapToRouteAction(action) {
    return {
      ...action,
      meta: {
        ...action.meta,
        $route: true,
      },
    };
  }

  function isRouteTransactionInProgress(store) {
    const state = store.getState();
    return state.operations.routePending > 1;
  }

  function routeTransactionEpic(action$, store) {
    return action$
      .ofType(BEGIN_ROUTE_TRANSACTION)
      .mergeMap((action) => {
        const actions = action.payload;
        if (actions.length === 0) {
          return Rx.Observable.of(endRouteTransaction());
        }
        return Rx.Observable.from(actions)
          .concatMap((actionOrActions) => {
            const stageActions = isArray(actionOrActions)
              ? actionOrActions
              : [actionOrActions];
            if (stageActions.length === 0) {
              return Rx.Observable.empty();
            }
            return Rx.Observable.from(stageActions)
              .map(mapToRouteAction)
              .concat(
                action$
                  .startWith(null)
                  .takeWhile(() => isRouteTransactionInProgress(store))
                  .ignoreElements(),
              );
          })
          .catch(e => Rx.Observable.of(endRouteTransaction()).throw(e))
          .concat([endRouteTransaction()]);
      });
  }


  return combineEpics(
    listenForServerEventsEpic,
    routeTransactionEpic,
    errors.createEpic(container),
    auth.createEpic(container),
    home.createEpic(container),
    room.createEpic(container),
  );
}

export default {
  createEpic,
  reducer,
};
