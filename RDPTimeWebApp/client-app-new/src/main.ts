import Vue from 'vue'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import store from './store'

import '@/filters';

import Buefy from 'buefy'
//import 'buefy/dist/buefy.css'
import '@/assets/scss/style.scss'
import '@mdi/font/css/materialdesignicons.min.css'

import moment from 'moment'
moment.locale("ru");

Vue.use(Buefy);

Vue.config.productionTip = false

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')
