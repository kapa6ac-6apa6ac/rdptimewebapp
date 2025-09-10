<template lang="pug">
#app
  b-navbar(type="is-success")
    template(#brand)
      b-navbar-item(tag="router-link", :to="{ path: '/' }")
        b Ara
    template(#start)
      b-navbar-item(
        tag="router-link",
        :to="{ path: '/' }",
        exact,
        active-class="is-active"
      )
        | Главная
      template(v-if="oidcIsAuthenticated")
        template(v-if="isViewAll || isAdmin")
          b-navbar-item(
            tag="router-link",
            :to="{ path: '/time' }",
            active-class="is-active"
          )
            | Сводная таблица
          b-navbar-item(
            tag="router-link",
            :to="{ path: '/users' }",
            active-class="is-active"
          )
            | Пользователи
        template(v-if="isAdmin")
          b-navbar-item(
            tag="router-link",
            :to="{ path: '/calendar' }",
            active-class="is-active"
          )
            | Календарь
          b-navbar-item(
            tag="router-link",
            :to="{ path: '/settings' }",
            active-class="is-active"
          )
            | Управление
    template(#end)
      b-navbar-dropdown(v-if="oidcIsAuthenticated", :label="userName")
        b-navbar-item(@click="logout")
          | Выйти
      b-navbar-item(tag="div")
        .buttons
          button.button.is-light(v-if="!oidcIsAuthenticated", @click="login")
            | Войти
  router-view
</template>

<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, Prop, Vue } from "vue-property-decorator";
import Oidc from "oidc-client";

@Component
export default class App extends Vue {
  @Getter("oidcIsAuthenticated", { namespace: "oidc" })
  oidcIsAuthenticated!: boolean;

  @Getter("oidcUser", { namespace: "oidc" })
  oidcUser!: Oidc.Profile;

  @Action("authenticateOidc", { namespace: "oidc" })
  authenticateOidc!: () => void;

  @Action("signOutOidcSilent", { namespace: "oidc" })
  signOutOidc!: () => void;

  private login() {
    this.authenticateOidc();
  }

  private logout() {
    this.signOutOidc();
  }

  private get userName() {
    return `${this.oidcUser.preferred_username} (${this.oidcUser.name})`;
  }

  private get isAdmin() {
    return this.oidcUser.role && this.oidcUser.role.includes("admin");
  }

  private get isViewAll() {
    return this.oidcUser.role && this.oidcUser.role.includes("view-all");
  }
}
</script>

<style lang="scss">
</style>
