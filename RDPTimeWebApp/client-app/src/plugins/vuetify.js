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
            },
        },
    },
    lang: {
        locales: { ru },
        current: 'ru',
    },
});
//# sourceMappingURL=vuetify.js.map