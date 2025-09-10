import Vue from 'vue'
import VueRouter, { RouteConfig } from 'vue-router'
import { vuexOidcCreateRouterMiddleware } from 'vuex-oidc'
import OidcCallback from '@/views/Oidc/OidcCallback.vue'
import OidcPopupCallback from '@/views/Oidc/OidcPopupCallback.vue'
import OidcCallbackError from '@/views/Oidc/OidcCallbackError.vue'
import store from '@/store'
import Home from '../views/Home.vue'

Vue.use(VueRouter)

const routes: Array<RouteConfig> = [
  {
    path: '/',
    name: 'Home',
    component: Home,
    meta: {
      isPublic: true
    }
  },
  {
    path: '/oidc-callback', // Needs to match redirectUri in you oidcSettings
    name: 'oidcCallback',
    component: OidcCallback
  },
  {
    path: '/oidc-popup-callback', // Needs to match popupRedirectUri in you oidcSettings
    name: 'oidcPopupCallback',
    component: OidcPopupCallback
  },
  {
    path: '/oidc-callback-error', // Needs to match redirect_uri in you oidcSettings
    name: 'oidcCallbackError',
    component: OidcCallbackError,
    meta: {
      isPublic: true
    }
  },
  {
    path: '/users',
    name: 'Users',
    component: () => import(/* webpackChunkName: "users" */ '@/views/Users.vue'),
    children: [
      {
        path: ":id",
        name: "ViewUser",
        component: () => import(/* webpackChunkName: "users" */ '@/views/Users/ViewUser.vue')
      }
    ]
  },
  {
    path: '/time',
    name: 'UsersTime',
    component: () => import(/* webpackChunkName: "userstime" */ '@/views/UsersTime.vue')
  },
  {
    path: '/calendar',
    name: 'Calendar',
    component: () => import(/* webpackChunkName: "calendar" */ '@/views/Calendar.vue')
  },
  {
    path: '/settings',
    name: 'Settings',
    component: () => import(/* webpackChunkName: "settings" */ '@/views/Settings.vue')
  },
  {
    path: '/about',
    name: 'About',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: "about" */ '../views/About.vue')
  }
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

router.beforeEach(vuexOidcCreateRouterMiddleware(store, 'oidc'));

export default router
