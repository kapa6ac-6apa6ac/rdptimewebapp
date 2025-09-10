<template lang="pug">
.container
  .mt-5.is-flex.is-flex-direction-row
    .is-flex-grow-1.is-size-2 {{ userName }}
    b-field.mt-4(label="Период", horizontal)
      c-datepicker(v-model="dates")
  b-tabs(v-model="activeTab")
    b-tab-item(label="Общее")
      c-view-daytime.mt-2(:user-id="$route.params.id", :dates="dates")
    b-tab-item(label="СКУД")
      c-view-scud.mt-2(:user-id="$route.params.id", :dates="dates")
    b-tab-item(label="RDP")
      c-view-connections.mt-2(:user-id="$route.params.id", :dates="dates")
    b-tab-item(label="Редактирование", v-if="isAdmin")
      c-edit-user(:user-id="$route.params.id")
</template>

<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, Vue } from "vue-property-decorator";
import moment from "moment";

import ViewConnections from "@/components/UserViews/ViewConnections.vue";
import ViewScud from "@/components/UserViews/ViewScud.vue";
import ViewDaytime from "@/components/UserViews/ViewDaytime.vue";
import { UserInfo } from "@/store/api/types";
import EditUser from "@/components/Users/EditUser.vue";
import Datepicker from "@/components/Datepicker.vue";

import Oidc from "oidc-client";

@Component({
  components: {
    "c-view-connections": ViewConnections,
    "c-view-scud": ViewScud,
    "c-view-daytime": ViewDaytime,
    "c-edit-user": EditUser,
    "c-datepicker": Datepicker,
  },
})
export default class ViewUser extends Vue {
  private dates: Date[] = [];
  private activeTab = 0;

  @Getter("users", { namespace: "api" })
  users!: UserInfo[];

  @Getter("oidcUser", { namespace: "oidc" })
  oidcUser!: Oidc.Profile;
  // @Action("authenticateOidcPopup", { namespace: "oidc" })
  // authenticateOidc!: () => void;

  created() {
    this.dates = [moment().subtract(7, "days").toDate(), moment().toDate()];
  }

  get userInfo() {
    const user = this.users.find(
      (u) => u.id == parseInt(this.$route.params.id)
    );
    if (user) return user;
    else return null;
  }

  get userName() {
    const user = this.userInfo;
    return user ? user.name : "-";
  }

  private get isAdmin() {
    return this.oidcUser.role && this.oidcUser.role.includes("admin");
  }
}
</script>