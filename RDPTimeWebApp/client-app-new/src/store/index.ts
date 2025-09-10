import Vue from 'vue'
import Vuex, { StoreOptions } from 'vuex'

import { vuexOidcCreateStoreModule } from 'vuex-oidc';
import { oidcSettings } from '../config/oidc';

import { api } from './api';

import { RootState } from './types'

Vue.use(Vuex)

const store: StoreOptions<RootState> = {
  state: {
    version: "0.1"
  },
  mutations: {
  },
  actions: {
  },
  modules: {
    api,
    oidc: vuexOidcCreateStoreModule(
      oidcSettings,
      {
        namespaced: true,
        dispatchEventsOnWindow: true
      },
      {
        userLoaded: (user) => console.log('OIDC user is loaded:', user),
        userUnloaded: () => console.log('OIDC user is unloaded'),
        accessTokenExpiring: () => console.log('Access token will expire'),
        accessTokenExpired: () => console.log('Access token did expire'),
        silentRenewError: () => console.log('OIDC user is unloaded'),
        userSignedOut: () => console.log('OIDC user is signed out'),
        oidcError: (payload) => console.log('OIDC error', payload),
        automaticSilentRenewError: (payload) => console.log('OIDC automaticSilentRenewError', payload)
      })
  }
}

export default new Vuex.Store<RootState>(store);