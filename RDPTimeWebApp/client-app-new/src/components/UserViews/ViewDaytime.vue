<template lang="pug">
.card
  b-loading(v-model="isLoading", :is-full-page="false")
  .card-content
    b-table(
      :data="data",
      default-sort="dateTime",
      :paginated="true",
      :per-page="perPage",
      sticky-header,
      :height="600"
    )
      b-table-column(
        field="date",
        label="Дата",
        subheading="Всего:",
        v-slot="props"
      )
        | {{ props.row.date | date }}
      b-table-column(
        field="timeScud",
        label="Время СКУД",
        :subheading="sumData((d) => d.timeScud) | hms",
        v-slot="props"
      )
        | {{ props.row.timeScud | hms(!fullHms) }} &nbsp;
        template(v-if="props.row.scudFrom") ({{ props.row.scudFrom | time }}&nbsp;-&nbsp;
          template(v-if="props.row.scudTo") {{ props.row.scudTo | time }})
          template(v-else) не вышел)
      b-table-column(
        field="timeScudA",
        label="Время СКУД (без обеда)",
        :subheading="sumData((d) => d.timeScudA) | hms",
        v-slot="props"
      )
        | {{ props.row.timeScudA | hms(!fullHms) }}
      b-table-column(
        field="timeRdp",
        label="Время RDP",
        :subheading="sumData((d) => d.timeRdp) | hms",
        v-slot="props"
      )
        | {{ props.row.timeRdp | hms(!fullHms) }}
      b-table-column(
        field="timeTotal",
        label="Время RDP + СКУД",
        :subheading="sumData((d) => d.timeScud + d.timeRdp) | hms",
        v-slot="props"
      )
        | {{ (props.row.timeRdp + props.row.timeScud) | hms(!fullHms) }}
      b-table-column(
        field="timeManic",
        label="Время ManicTime",
        :subheading="sumData((d) => d.timeManic) | hms",
        v-slot="props"
      )
        | {{ props.row.timeManic | hms(!fullHms) }}
      b-table-column(
        field="timeVector",
        label="Время Vector",
        :subheading="sumData((d) => d.timeVector) | hms",
        v-slot="props"
      )
        | {{ props.row.timeVector | hms(!fullHms) }}

      template(#bottom-left)
        b-select(v-model="perPage")
          option(value="15") 15 на странице
          option(value="50") 50 на странице
        b-checkbox.ml-3(v-model="fullHms") Точное время
</template>

<script lang="ts">
import { DayTimeInfo } from "@/store/api/types";
import { Action } from "vuex-class";
import { Component, Prop, Vue, Watch } from "vue-property-decorator";
import moment from "moment";

@Component
export default class ViewDaytime extends Vue {
  private isLoading = true;
  private data: DayTimeInfo[] = [];
  private abortController!: AbortController;

  private perPage = 15;
  private fullHms = false;

  @Action("getMyDayTime", { namespace: "api" })
  getMyDayTime!: (payload: {
    dates: Date[];
    signal: AbortSignal;
  }) => Promise<DayTimeInfo[]>;

  @Action("getUserDayTime", { namespace: "api" })
  getUserDayTime!: (payload: {
    dates: Date[];
    userId: number;
    signal: AbortSignal;
  }) => Promise<DayTimeInfo[]>;

  @Prop() private dates!: Date[];
  @Prop({ default: null }) private userId!: number | null;

  @Watch("dates")
  @Watch("userId")
  private async onDatesChanged() {
    await this.fetchData();
  }

  private created() {
    this.abortController = new AbortController();
  }

  private async mounted() {
    this.fetchData();
  }

  private async fetchData() {
    this.abortController.abort();
    this.abortController = new AbortController();

    this.isLoading = true;
    const signal = this.abortController.signal;
    const userId = this.userId;
    const dates = this.dates;
    let data: DayTimeInfo[] = [];
    try {
      if (userId == null) {
        if (dates.length == 1) {
          data = await this.getMyDayTime({
            dates: [dates[0], dates[0]],
            signal,
          });
        } else if (dates.length == 2) {
          data = await this.getMyDayTime({ dates, signal });
        }
      } else {
        if (dates.length == 1) {
          data = await this.getUserDayTime({
            dates: [dates[0], dates[0]],
            userId,
            signal,
          });
        } else if (dates.length == 2) {
          data = await this.getUserDayTime({
            dates,
            userId,
            signal,
          });
        }
      }
      if (
        this.userId === userId &&
        this.dates.length === dates.length &&
        this.dates[0] === dates[0] &&
        (dates.length === 1 || this.dates[1] === dates[1])
      ) {
        this.data = data;
        this.isLoading = false;
      }
    } catch (e) {
      if (e instanceof Error) {
        if (e.name != "AbortError") {
          this.data = [];
          this.isLoading = false;
        }
      }
    }
  }

  private sumData(func: (info: DayTimeInfo) => number, init = 0) {
    const sum = this.data.reduce((prev, curr) => prev + func(curr), init);
    return sum;
  }
}
</script>