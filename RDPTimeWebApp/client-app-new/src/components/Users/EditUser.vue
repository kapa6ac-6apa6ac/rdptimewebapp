<template lang="pug">
.card
  b-loading(v-model="isLoading", :is-full-page="false")
  .card-content.is-centered.is-size-4(v-if="user == null")
    | Загрузка
  .card-content(v-else)
    b-field(label="ID")
      b-input(v-model="user.id", disabled)
    b-field(label="ФИО")
      b-input(v-model="user.name")
    b-field(label="Логин AD")
      b-input(v-model="user.login")
    b-field(label="СКУД ID (Салават)")
      b-input(v-model="user.scudSlvId", type="number")
    b-field(label="СКУД ID (Уфа)")
      b-input(v-model="user.scudUfaId", type="number")
    b-button(type="is-primary", @click="save") Сохранить
</template>

<script lang="ts">
import { UserInfo } from "@/store/api/types";
import { Action, Getter } from "vuex-class";
import { Component, Prop, Vue, Watch } from "vue-property-decorator";

@Component
export default class EditUser extends Vue {
  private isLoading = true;

  @Action("saveUser", { namespace: "api" })
  saveUser!: (user: UserInfo) => Promise<void>;

  // @Action("getUserDayTime", { namespace: "api" })
  // getUserDayTime!: (payload: {
  //   dates: Date[];
  //   userId: number;
  // }) => Promise<DayTimeInfo[]>;
  @Getter("users", { namespace: "api" })
  users!: UserInfo[];

  @Prop() private userId!: number;
  private user: UserInfo | null = null;

  get userInfo() {
    this.users.length;
    const user = this.users.find(
      (u) => u.id == parseInt(this.$route.params.id)
    );
    
    if (user) return user;
    else return null;
  }

  @Watch("userInfo")
  private onUserChange(val: UserInfo) {
    this.user = Object.assign({}, val);
    this.isLoading = this.user == null;
  }

  private async save() {
    this.isLoading = true;
    try {
      await this.saveUser(this.user!);
    } catch {
      /* empty */
    }
    this.isLoading = false;
    // empty
  }
}
</script>