<template>
  <div class="container">
    <h1>This is an about page</h1>
    <pre>{{ info | pretty }}</pre>
  </div>
</template>

<script lang="ts">
import { Component } from "vue-property-decorator";
import Vue from "vue";
import { Getter } from "vuex-class";

@Component({
  filters: {
    pretty: function (value: any) {
      return JSON.stringify(value, null, 2);
    },
  },
})
export default class About extends Vue {
  // eslint-disable-next-line
  info: any = {};

  @Getter("oidcAccessToken", { namespace: "oidc" })
  oidcAccessToken!: string;

  async mounted(): Promise<void> {
    const info = await fetch("/api/v2/me/time/rdp", {
      headers: {
        Authorization: `Bearer ${this.oidcAccessToken}`,
      },
    });
    this.info = await info.json();
  }
}
</script>