<template>
  <v-card :loading="dataLoading">
    <v-card-title>{{date}} - {{user}} - {{name}}</v-card-title>
    <v-tabs>
      <v-tab href="#rdp">RDP</v-tab>
      <v-tab href="#scud">СКУД</v-tab>
      <v-tab-item value="rdp">
        <v-data-table :headers="rdpHeaders" :items="rdpLogs" :items-per-page="10">
          <template
            v-slot:item.starttime="{ item }"
          >{{formatDate(new Date(item.time) - (item.duration * 1000))}}</template>
          <template v-slot:item.time="{ item }">{{formatDate(new Date(item.time))}}</template>
          <template v-slot:item.duration="{ item }">{{secondsToTime(item.duration)}}</template>
        </v-data-table>
      </v-tab-item>
      <v-tab-item value="scud">
        <v-data-table :headers="scudHeaders" :items="scudLogs" :items-per-page="10">
          <template v-slot:item.mode="{ item }">
            <v-chip
              text-color="white"
              :color="item.mode == 1 ? 'green' : 'red'"
            >{{modes[item.mode]}}</v-chip>
          </template>
          <template v-slot:item.time="{ item }">{{formatDate(new Date(item.time))}}</template>
          <template v-slot:item.door="{ item }">{{cities[item.city].doors[item.door]}}</template>
          <template v-slot:item.event="{ item }">{{events[item.event]}}</template>
          <template v-slot:item.city="{ item }">{{cities[item.city].name}}</template>
        </v-data-table>
      </v-tab-item>
    </v-tabs>
  </v-card>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  name: "user-logs",
  props: {
    userId: Number,
    date: String
  },
  data: () => ({
    rdpHeaders: [
      { text: "С", value: "starttime" },
      { text: "До", value: "time" },
      { text: "Продолжительность", value: "duration" },
      { text: "Компьютер", value: "computer" }
    ],
    scudHeaders: [
      { text: "Тип", value: "mode" },
      { text: "Время", value: "time" },
      { text: "Вход", value: "door" },
      { text: "Событие", value: "event" },
      { text: "Город", value: "city" }
    ],
    modes: {
      1: "Вход",
      2: "Выход"
    },
    events: {
      1: "Доступ предоставлен",
      2: "Запрет доступа",
      0: "Проход"
    },
    cities: {
      1: {
        name: "Салават",
        doors: {
          8: "л.кр.1эт.вых.дальний",
          11: "1эт.ц.выход тыл",
          13: "п.кр.1эт. вых.фасад",
          50: "Турникет 1",
          51: "Турникет 2"
        }
      },
      2: {
        name: "Уфа",
        doors: {
          4: "Турникет 1",
          5: "Турникет 2"
        }
      }
    },

    name: "",
    user: "",
    rdpLogs: [],
    scudLogs: [],
    dataLoading: false
  }),
  methods: {
    loadData() {
      const date = this.date.replace("-", "/").replace("-", "/");
      this.dataLoading = true;
      this.name = "";
      this.user = "";
      this.rdpLogs = [];
      this.scudLogs = [];
      fetch("/api/logs/" + this.userId + "/" + date)
        .then(response => response.json())
        .then(data => {
          this.name = data.name;
          this.user = data.user;
          this.rdpLogs = data.rdpLogs;
          this.scudLogs = data.scudLogs;
          this.dataLoading = false;
        });
    },
    formatDate(time: Date) {
      return new Intl.DateTimeFormat("ru", {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "numeric",
        minute: "numeric",
        second: "numeric"
      }).format(time);
    },
    secondsToTime(seconds: number): string {
      if (seconds == undefined) return "-";

      const h = ~~(seconds / 3600);
      const m = ~~((seconds - h * 3600) / 60);
      const s = seconds - h * 3600 - m * 60;

      const date = new Date(0);
      date.setSeconds(seconds); // specify value for SECONDS here
      return `${h}:${m.toString().padStart(2, "0")}:${s
        .toString()
        .padStart(2, "0")}`;
    }
  },
  watch: {
    userId: function(value, oldValue) {
      if (this.userId != 0) this.loadData();
    }
  },
  mounted() {
    if (this.userId != 0) this.loadData();
  }
});
</script>