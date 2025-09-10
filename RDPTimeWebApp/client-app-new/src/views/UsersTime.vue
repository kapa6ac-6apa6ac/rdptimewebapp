<template lang="pug">
.container.is-fluid
  .mt-5.is-flex.is-flex-direction-row
    .is-size-2 Сводная таблица
    .mt-4.ml-3
      b-button.is-success(
        @click="downloadCommonReport",
        icon-left="file-excel-outline"
      )
        | Скачать отчет
    .is-flex-grow-1
    .mt-4.is-flex
      b-field(label="Поиск", horizontal)
        b-input(v-model="search")
      b-field.ml-5(label="Период", horizontal)
        c-datepicker(v-model="dates")
  .card
    b-loading(v-model="isLoading", :is-full-page="false")
    .card-content
      b-table(
        :data="users",
        default-sort="login",
        :paginated="true",
        :per-page="perPage",
        sticky-header,
        :height="700"
      )
        b-table-column(field="login", label="Логин", v-slot="props", sortable)
          | {{ props.row.login }}
        b-table-column(field="name", label="ФИО", v-slot="props", sortable)
          | {{ props.row.name }}
        b-table-column(
          field="timeScud",
          label="Время СКУД",
          v-slot="props",
          sortable
        )
          | {{ props.row.timeScud | hms(!fullHms) }} &nbsp;
          template(v-if="props.row.scudFrom") ({{ props.row.scudFrom | time }}&nbsp;-&nbsp;
            template(v-if="props.row.scudTo") {{ props.row.scudTo | time }})
            template(v-else) не вышел)
        b-table-column(
          field="timeScudA",
          label="Время СКУД (без обеда)",
          v-slot="props",
          sortable
        )
          | {{ props.row.timeScudA | hms(!fullHms) }}
        b-table-column(
          field="timeRdp",
          label="Время RDP",
          v-slot="props",
          sortable
        )
          | {{ props.row.timeRdp | hms(!fullHms) }}
        b-table-column(
          field="timeTotal",
          label="Время RDP + СКУД",
          v-slot="props"
        )
          | {{ (props.row.timeRdp + props.row.timeScud) | hms(!fullHms) }}
        b-table-column(
          :visible="loadManicTime",
          field="timeManic",
          label="Время ManicTime",
          v-slot="props",
          sortable
        )
          | {{ props.row.timeManic | hms(!fullHms) }}
        b-table-column(
          field="timeVector",
          label="Время Vector",
          v-slot="props",
          sortable
        )
          | {{ props.row.timeVector | hms(!fullHms) }}

        template(#bottom-left)
          b-select(v-model="perPage")
            option(value="15") 15 на странице
            option(value="50") 50 на странице
            option(value="100") 100 на странице
            option(value="999999") все записи <!-- KEK -->
          b-checkbox.ml-3(v-model="fullHms") Точное время
          b-checkbox.ml-3(v-model="allUsers") Все пользователи
          b-checkbox.ml-3(v-model="loadManicTime") ManicTime
</template>

<script lang="ts">
import { Component, Watch } from "vue-property-decorator";
import Vue from "vue";
import moment from "moment";
import { DayTimeInfo } from "@/store/api/types";
import { Action } from "vuex-class";
import Datepicker from "@/components/Datepicker.vue";

@Component({
  components: {
    "c-datepicker": Datepicker,
  },
})
export default class UsersTime extends Vue {
  private isLoading = true;
  private dates: Date[] = [];
  private data: DayTimeInfo[] = [];

  private search = "";
  private perPage = 15;
  private fullHms = true;
  private allUsers = false;
  private loadManicTime = false;

  @Action("getUsersTime", { namespace: "api" })
  private getUsersTime!: (payload: {
    dates: Date[];
    manicTime: boolean;
  }) => Promise<DayTimeInfo[]>;

  @Action("downloadCommonReport", { namespace: "api" })
  private downloadCommonReport_!: (payload: {
    from: Date;
    to: Date;
    allUsers: boolean;
    manicTime: boolean;
  }) => Promise<void>;

  private created() {
    this.dates = [moment().subtract(7, "days").toDate(), moment().toDate()];
  }

  private async mounted() {
    await this.fetchData();
  }

  @Watch("loadManicTime")
  @Watch("dates")
  private async fetchData() {
    this.isLoading = true;
    if (this.dates.length == 1) {
      this.data = await this.getUsersTime({
        dates: [this.dates[0], this.dates[0]],
        manicTime: this.loadManicTime,
      });
    } else if (this.dates.length == 2) {
      this.data = await this.getUsersTime({
        dates: this.dates,
        manicTime: this.loadManicTime,
      });
    }
    this.isLoading = false;
  }

  private async downloadCommonReport() {
    const loadingComponent = this.$buefy.loading.open({ isFullPage: true });
    await this.downloadCommonReport_({
      from: this.dates[0],
      to: this.dates.length == 2 ? this.dates[1] : this.dates[0],
      allUsers: this.allUsers,
      manicTime: this.loadManicTime,
    });
    loadingComponent.close();
  }

  private get users() {
    let users = this.data;

    if (this.search.length > 0) {
      let search = this.search.toLowerCase();
      users = users.filter(
        (u) => u.name?.toLowerCase().includes(search) || u.login?.toLowerCase().includes(search)
      );
    }

    if (!this.allUsers)
      users = users.filter(
        (u) =>
          u.timeRdp > 0 ||
          u.timeScud > 0 ||
          (u.timeManic && u.timeManic > 0) ||
          u.timeVector > 0 ||
          u.scudFrom != undefined
      );

    return users;
  }
}
</script>

<style lang="scss" scoped>
</style>