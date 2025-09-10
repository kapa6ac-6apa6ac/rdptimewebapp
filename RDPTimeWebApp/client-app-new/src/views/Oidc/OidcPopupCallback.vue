<template lang="pug">
  div
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { Action } from "vuex-class";

@Component({
  name: "OidcPopupCallback",
})
export default class OidcCallback extends Vue {
  @Action("oidcSignInPopupCallback", { namespace: "oidc" })
  oidcSignInPopupCallback!: () => Promise<any>;

  created() {
    this.oidcSignInPopupCallback()
      .catch((err) => {
        console.error(err);
        this.$router.push("/signin-oidc-error");
      });
  }
}
</script>