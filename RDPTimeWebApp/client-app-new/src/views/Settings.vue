<template lang="pug">
.container
  .is-size-2 Настройки
  .card
    .card-header
      p.card-header-title СКУД
    .card-content
      .buttons
        b-button(@click="syncScudSlv", :loading="isScudSyncSlv") Синхронизация с Салаватским СКУД
        b-button(@click="syncScudUfa", :loading="isScudSyncUfa") Синхронизация с Уфимским СКУД
</template>

<script lang="ts">
import { Component } from "vue-property-decorator";
import { Action } from "vuex-class";
import Vue from "vue";

@Component
export default class Settings extends Vue {
  private isScudSyncSlv = false;
  private isScudSyncUfa = false;

  @Action("syncScudSlv", { namespace: "api" })
  private _syncScudSlv!: () => Promise<void>;

  @Action("syncScudUfa", { namespace: "api" })
  private _syncScudUfa!: () => Promise<void>;

  private async syncScudSlv() {
    this.isScudSyncSlv = true;
    //await this.sleep(2500);
    await this._syncScudSlv();
    console.log("slv");
    this.isScudSyncSlv = false;
  }

  private async syncScudUfa() {
    this.isScudSyncUfa = true;
    //await this.sleep(2500);
    await this._syncScudUfa();
    this.isScudSyncUfa = false;
  }

  sleep(ms: number) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
</script>