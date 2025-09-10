<template>
  <v-data-table
    :headers="headers"
    :items="users"
    :search="search"
    sort-by="name"
  >
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>Пользователи</v-toolbar-title>
        <v-divider class="mx-4" inset vertical></v-divider>

        <v-dialog v-model="dialog" max-width="500px">
          <template v-slot:activator="{ on }">
            <v-btn text class="mr-2" v-on="on">
              <v-icon class="mr-2">mdi-account-plus</v-icon>Создать
            </v-btn>
          </template>
          <v-card>
            <v-card-title>
              <span class="headline">{{ formTitle }}</span>
            </v-card-title>

            <v-card-text>
              <v-container>
                <v-row>
                  <v-col cols="12" sm="12" md="12">
                    <v-text-field
                      v-model="editedItem.name"
                      label="ФИО"
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="12" md="12">
                    <v-text-field
                      v-model="editedItem.login"
                      label="Логин AD"
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="6" md="6">
                    <v-text-field
                      v-model.number="editedItem.scudSlvId"
                      type="number"
                      label="СКУД ID (Салават)"
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="6" md="6">
                    <v-text-field
                      v-model.number="editedItem.scudUfaId"
                      type="number"
                      label="СКУД ID (Уфа)"
                    ></v-text-field>
                  </v-col>
                </v-row>
              </v-container>
            </v-card-text>

            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue darken-1" text @click="close">Отмена</v-btn>
              <v-btn color="blue darken-1" text @click="save">Сохранить</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>

        <v-spacer></v-spacer>
        <v-text-field
          style="max-width: 300px"
          v-model="search"
          append-icon="mdi-magnify"
          label="Поиск"
          single-line
          hide-details
        ></v-text-field>
      </v-toolbar>
    </template>
    <template v-slot:item.actions="{ item }">
      <v-icon small class="mr-2" @click="editItem(item)">mdi-pencil</v-icon>
      <!-- <v-icon small @click="deleteItem(item)">mdi-delete</v-icon> -->
    </template>
  </v-data-table>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  data: () => ({
    search: "",
    dialog: false,
    defaultItem: {
      name: "",
      login: "",
      scudSlvId: 0,
      scudUfaId: 0,
    },
    editedItem: {} as any,
    editedIndex: -1,
    headers: [
      { text: "ID", value: "id" },
      { text: "ФИО", value: "name" },
      { text: "Логин AD", value: "login" },
      { text: "СКУД ID (Салават)", value: "scudSlvId" },
      { text: "СКУД ID (Уфа)", value: "scudUfaId" },
      { text: "Действия", value: "actions", sortable: false },
    ],
    users: [],
  }),
  mounted() {
    this.editedItem = this.defaultItem;
    fetch("/api/user")
      .then((data) => data.json())
      .then((data) => (this.users = data));
  },
  methods: {
    editItem(item: any) {
      this.editedIndex = this.users.indexOf(item as never);
      this.editedItem = Object.assign({}, item);
      this.dialog = true;
    },
    close() {
      this.dialog = false;
      this.$nextTick(() => {
        this.editedItem = Object.assign({}, this.defaultItem);
        this.editedIndex = -1;
      });
    },
    save() {
      if (this.editedIndex > -1) {
        fetch(`/api/user/${this.editedItem.id}`, {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(this.editedItem),
        }).then((data) => {
          Object.assign(this.users[this.editedIndex], this.editedItem);
          this.close();
        });
      } else {
        fetch(`/api/user/`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(this.editedItem),
        })
          .then((data) => data.json())
          .then((data) => {
            this.users.push(data as never);
            this.close();
          });
      }
    },
  },
  computed: {
    formTitle() {
      return this.editedIndex === -1
        ? "Новый пользователь"
        : "Редактирование пользователя";
    },
  },
  watch: {
    dialog(val) {
      val || this.close();
    },
  },
});
</script>