<template lang="pug">
.container
  template(v-if="!oidcIsAuthenticated")
    .has-text-centered.mt-5
      .is-size-2 Войдите для просмотра времени
      button.button.is-success(@click="authenticateOidc") Войти
  template(v-else)
    .mt-5.is-flex.is-flex-direction-row
      .is-flex-grow-1.is-size-2 Ваше время
      b-field.mt-4(label="Период", horizontal)
        c-datepicker(v-model="dates")
    b-tabs(v-model="activeTab")
      b-tab-item(label="Общее")
        c-view-daytime.mt-2(:dates="dates")
      b-tab-item(label="СКУД")
        c-view-scud.mt-2(:dates="dates")
      b-tab-item(label="RDP")
        c-view-connections.mt-2(:dates="dates")
</template>

<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, Vue } from "vue-property-decorator";
import moment from "moment";

import ViewConnections from "@/components/UserViews/ViewConnections.vue";
import ViewScud from "@/components/UserViews/ViewScud.vue";
import ViewDaytime from "@/components/UserViews/ViewDaytime.vue";
import Datepicker from "@/components/Datepicker.vue";

@Component({
  components: {
    "c-view-connections": ViewConnections,
    "c-view-scud": ViewScud,
    "c-view-daytime": ViewDaytime,
    "c-datepicker": Datepicker,
  },
})
export default class Home extends Vue {
  private dates: Date[] = [];
  private activeTab = 0;

  @Getter("oidcIsAuthenticated", { namespace: "oidc" })
  oidcIsAuthenticated!: boolean;

  @Action("authenticateOidc", { namespace: "oidc" })
  authenticateOidc!: () => void;

  created() {
    this.dates = [moment().subtract(7, "days").toDate(), moment().toDate()];
  }
}
</script>
