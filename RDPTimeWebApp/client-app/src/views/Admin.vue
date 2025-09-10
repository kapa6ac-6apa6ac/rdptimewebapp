<template>
  <v-container style="height: 100%">
    <v-row
      v-if="pass != '1Qwerty!@#$'"
      style="height: 100%"
      align="center"
      justify="center"
    >
      <v-col class="mx-auto" style="max-width: 400px">
        <v-card>
          <v-card-title>Вход</v-card-title>
          <v-card-text>
            <v-text-field
              outlined
              v-model="pass"
              hide-details
              type="password"
              label="Пароль"
            ></v-text-field>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
    <v-card v-else>
      <v-toolbar color="primary" dark flat>
        <v-toolbar-title>Администрирование</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="red" @click="pass = ''">Выйти</v-btn>

        <template v-slot:extension>
          <v-tabs v-model="activeTab">
            <v-tab key="sync">Синхронизация</v-tab>
            <v-tab key="users">Пользователи</v-tab>
          </v-tabs>
        </template>
      </v-toolbar>
      <v-tabs-items v-model="activeTab">
        <v-tab-item key="sync">
          <v-card-text>
            <v-btn
              class="mr-2"
              :loading="isSyncScudSalavat"
              @click="syncSCUD('salavat')"
              >Синхронизация с салаватским СКУД</v-btn
            >
            <v-btn :loading="isSyncScudUfa" @click="syncSCUD('ufa')"
              >Синхронизация с уфимским СКУД</v-btn
            >
          </v-card-text>
        </v-tab-item>
        <v-tab-item key="users">
          <c-editusers></c-editusers>
        </v-tab-item>
      </v-tabs-items>
    </v-card>
  </v-container>
</template>

<script lang="ts">
import Vue from "vue";
import EditUsers from "./EditUsers.vue";

export default Vue.extend({
  components: {
    "c-editusers": EditUsers,
  },
  data: () => ({
    pass: "",
    date: new Date().toISOString().substr(0, 7),
    menu: false,
    file: (null as unknown) as Blob,
    activeTab: "sync",
    isSyncScudSalavat: false,
    isSyncScudUfa: false,
  }),
  methods: {
    uploadVectorData() {
      const date = this.date.replace("-", "/").replace("-", "/");
      const formData = new FormData();
      formData.append("file", (this.file as unknown) as Blob);
      fetch("/api/vectordataupload/" + date, {
        method: "post",
        body: formData,
      })
        .then((r) => {
          this.file = (null as unknown) as Blob;
        })
        .catch(console.error);
    },
    syncSCUD(param: string) {
      if (param == "salavat") this.isSyncScudSalavat = true;
      else this.isSyncScudUfa = true;
      fetch("/api/syncscud/" + param, {
        method: "post",
      }).then((e) => {
        if (param == "salavat") this.isSyncScudSalavat = false;
        else this.isSyncScudUfa = false;
      });
    },
  },
  mounted() {
    const pass = localStorage.getItem("adminPass");
    if (pass !== null) this.pass = pass;
  },
  watch: {
    pass: function (val) {
      localStorage.setItem("adminPass", val);
    },
  },
});
</script>