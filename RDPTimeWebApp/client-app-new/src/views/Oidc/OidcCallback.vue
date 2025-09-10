<template lang="pug">
div
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { Action } from "vuex-class";

@Component({
  name: "OidcCallback",
})
export default class OidcCallback extends Vue {
  @Action("oidcSignInCallback", { namespace: "oidc" })
  oidcSignInCallback!: () => Promise<string>;

  created() {
    this.oidcSignInCallback()
      .then((redirectPath) => {
        this.$router.push(redirectPath);
      })
      .catch((err) => {
        console.error(err);
        this.$router.push("/signin-oidc-error");
      });
  }
}
</script>