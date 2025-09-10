<template lang="pug">
.is-flex
  c-users-list.users-list.mx-2.my-2
  .user-info.is-flex-grow-1
    router-view
</template>

<script lang="ts">
import { Component } from "vue-property-decorator";
import Vue from "vue";
import UsersList from "@/components/Users/UsersList.vue";

@Component({
  components: {
    "c-users-list": UsersList,
  },
})
export default class Users extends Vue {}
</script>

<style lang="scss" scoped>
.users-list {
  min-width: 300px;
}
</style>