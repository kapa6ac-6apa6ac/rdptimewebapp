import Vue from 'vue';
import Vuetify from 'vuetify/lib';
import ru from 'vuetify/src/locale/ru';

Vue.use(Vuetify);

export default new Vuetify({
  theme: {
    themes: {
      light: {
        primary: '#8bc34a',
        secondary: '#1976d2',
        //accent: '#82B1FF',
        //error: '#FF5252',
        //info: '#2196F3',
        //success: '#4CAF50',
        //warning: '#FFC107'
      },
    },
  },
    lang: {
      locales: { ru },
      current: 'ru',
    },
});
