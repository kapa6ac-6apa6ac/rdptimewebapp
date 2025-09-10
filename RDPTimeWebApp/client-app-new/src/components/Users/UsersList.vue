<template lang="pug">
aside.menu
  b-field
    b-input(v-model="filter", placeholder="Поиск")
  ul.menu-list(
    style="overflow-y: auto; max-height: calc(100vh - 52px - 40px - 2rem)"
  )
    li(v-for="user in filteredUsers")
      router-link(
        :to="{ name: 'ViewUser', params: { id: user.id } }",
        active-class="is-active"
      )
        div {{ user.name }}
        div {{ user.login }}
</template>

<script lang="ts">
import { Component } from "vue-property-decorator";
import Vue from "vue";
import { Action, Getter } from "vuex-class";
import { UserInfo } from "@/store/api/types";

@Component
export default class UsersList extends Vue {
  private isLoading = true;
  private filter = "";

  @Getter("users", { namespace: "api" })
  private users!: UserInfo[];  

  @Action("loadUsers", { namespace: "api" })
  loadUsers!: () => Promise<void>;

  private async fetchData() {
    this.isLoading = true;
    await this.loadUsers();
    this.isLoading = false;
  }

  private async mounted() {
    await this.fetchData();
  }

  private get filteredUsers() {
    let users = this.users;

    if (this.filter.length > 0) {
      const filter = this.filter.toLowerCase();
      users = users.filter(
        (u) =>
          u.name.toLowerCase().includes(filter) ||
          u.login.toLowerCase().includes(filter)
      );
    }

    users = users.sort((a, b) => {
      if (a.name > b.name) return 1;
      if (a.name < b.name) return -1;
      return 0;
    });

    return users;
  }
}
</script>