<template>
  <v-container fluid>
    <v-row no-gutters style="flex-wrap: nowrap;">
      <v-col
        cols="1"
        class="flex-grow-1 flex-shrink-0 pr-3"
        style="min-width: 300px; max-width: 100%;"
      >
        <v-data-table
          :headers="dataHeaders"
          :items="data"
          :loading="dataLoading"
          :items-per-page="-1"
          sort-by="user"
          :search="searchFilter"
          :custom-sort="customSort"
        >
          <template v-slot:item.user="{ item }">
            <a @click="selectUser(item.id)">{{item.user}}</a>
          </template>
          <template v-slot:item.timeRDP="{ item }">{{secondsToTime(item.timeRDP)}}</template>
          <template
            v-slot:item.timeSCUD="{ item }"
          >{{secondsToTime(item.timeSCUD)}} {{scudTime(item)}}</template>
          <template v-slot:item.timeSCUD_R="{ item }"
            >{{ secondsToTime(item.timeSCUD_R) }}</template
          >
          <template v-slot:item.timeALL="{ item }">{{secondsToTime(item.timeRDP + item.timeSCUD)}}</template>
          <template v-slot:item.timeVector="{ item }">{{secondsToTime(item.timeVector)}}</template>
        </v-data-table>
      </v-col>
      <v-col cols="1" class="flex-grow-0 flex-shrink-1" style="min-width: 290px;">
        <v-text-field
          solo
          v-model="searchFilter"
          prepend-inner-icon="mdi-magnify"
          clearable
          single-line
          hide-details
          class="mb-2"
          label="Поиск"
        ></v-text-field>
        <v-date-picker
          v-model="date"
          :first-day-of-week="1"
          :type="pickerType"
          :scrollable="true"
          :disabled="dataLoading"
          color="#ffa000"
          header-color="primary"
        >
          <v-radio-group class="mt-0" :disabled="dataLoading" v-model="pickerType">
            <v-radio label="День" value="date" />
            <v-radio label="Месяц" value="month" />
          </v-radio-group>
        </v-date-picker>
        <v-checkbox v-model="onlyNoExit" label="Только невышедшие"></v-checkbox>
        <v-btn @click="dlReport()" color="primary" block class="mt-2">Скачать отчет за {{date}}</v-btn>
      </v-col>
    </v-row>
    <v-dialog v-model="isUserSelected" max-width="1200">
      <user-logs v-bind:userId="selectedUser" v-bind:date="date" />
    </v-dialog>
  </v-container>
</template>

<script lang="ts">
import Vue from "vue";
import UserLogs from "../components/UserLogs.vue";

export default Vue.extend({
  name: "Home",
  components: {
    UserLogs
  },
  data: () => ({
    data: [],
    dataLoading: true,
    searchFilter: "",
    onlyNoExit: false,
    date: new Date().toISOString().substr(0, 10),
    pickerType: "date",
    selectedUser: 0
  }),
  methods: {
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
    },
    loadData() {
      const date = this.date.replace("-", "/").replace("-", "/");
      this.dataLoading = true;
      this.data = [];
      fetch("/api/time/" + date)
        .then(response => response.json())
        .then(data => {
          this.data = data.timeData;
          this.dataLoading = false;
        });
    },
    dlReport() {
      const date = this.date.replace("-", "/").replace("-", "/");
      window.open("/api/report/" + date, "_blank");
    },
    scudTime(item: any): string {
      if (item.scudStart == undefined || item.scudStart == "") return "";
      else if (item.scudEnd == null) return `(${item.scudStart}-не вышел)`;
      return `(${item.scudStart}-${item.scudEnd})`;
    },
    selectUser(id: number) {
      this.selectedUser = id;
    },
    customSort(items: any, index: string[], isDesc: boolean[]) {
      items.sort((a: any, b: any) => {
        if (index[0] === "timeALL") {
          if (!isDesc[0]){
            return a["timeRDP"] + a["timeSCUD"] < b["timeRDP"] + b["timeSCUD"] ? -1 : 1;
          } else {
            return b["timeRDP"] + b["timeSCUD"] < a["timeRDP"] + a["timeSCUD"] ? -1 : 1;
          }
        } else {
          if (!isDesc[0]) {
            return a[index[0]] < b[index[0]] ? -1 : 1;
          } else {
            return b[index[0]] < a[index[0]] ? -1 : 1;
          }
        }
      });
      return items;
    }
  },
  computed: {
    isUserSelected: {
      get: function() {
        return this.selectedUser != 0;
      },
      set: function(value: boolean) {
        if (value == false) this.selectedUser = 0;
      }
    },
    dataHeaders() {
      const headers = [
        { text: "Пользователь", value: "user" },
        { text: "ФИО", value: "name" },
        { text: "Время RDP", value: "timeRDP" },
        {
          text: "Время СКУД",
          value: "timeSCUD",
          filter: (value: any, search: string | null, item: any) => {
            if (this.onlyNoExit)
              return item.scudStart != null && item.scudEnd == null;
            return true;
          }
        },
        {
          text: "Время СКУД (без обеда)",
          value: "timeSCUD_R"
        },
        { text: "Время RDP+СКУД", value: "timeALL" }
      ];

      //if (this.pickerType == "month")
        headers.push({ text: "Время Vector", value: "timeVector" });

      return headers;
    }
  },
  mounted() {
    this.loadData();
  },
  watch: {
    date: function() {
      this.loadData();
    }
  }
});
</script>
