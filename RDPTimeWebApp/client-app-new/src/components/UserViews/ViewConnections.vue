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
      b-table-column(field="fromTime", label="С", v-slot="props")
        | {{ getFromTime(props.row) | datetime }}
      b-table-column(field="dateTime", label="До", sortable, v-slot="props")
        | {{ props.row.dateTime | datetime }}
      b-table-column(
        field="time",
        label="Продолжительность",
        sortable,
        v-slot="props"
      )
        | {{ props.row.time | hms(!fullHms) }}
      b-table-column(field="computer", label="Компьютер", v-slot="props")
        | {{ props.row.computer }}
      b-table-column(field="ipAddress", label="IP адрес", v-slot="props")
        | {{ props.row.ipAddress }}
      template(#bottom-left)
        b-select(v-model="perPage")
          option(value="15") 15 на странице
          option(value="50") 50 на странице
        b-checkbox.ml-3(v-model="fullHms") Точное время
</template>

<script lang="ts">
import { ConnectionInfo } from "@/store/api/types";
import { Action } from "vuex-class";
import { Component, Prop, Vue, Watch } from "vue-property-decorator";
import moment from "moment";

@Component
export default class ViewConnections extends Vue {
  private isLoading = true;
  private data: ConnectionInfo[] = [];
  private abortController!: AbortController;

  private perPage = 15;
  private fullHms = false;

  @Action("getMyConnectionTime", { namespace: "api" })
  getMyConnectionTime!: (payload: {
    dates: Date[];
    signal: AbortSignal;
  }) => Promise<ConnectionInfo[]>;

  @Action("getUserConnectionTime", { namespace: "api" })
  getUserConnectionTime!: (payload: {
    dates: Date[];
    userId: number;
    signal: AbortSignal;
  }) => Promise<ConnectionInfo[]>;

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
    let data: ConnectionInfo[] = [];
    try {
      if (userId == null) {
        if (dates.length == 1) {
          data = await this.getMyConnectionTime({
            dates: [dates[0], dates[0]],
            signal,
          });
        } else if (this.dates.length == 2) {
          data = await this.getMyConnectionTime({ dates, signal });
        }
      } else {
        if (dates.length == 1) {
          data = await this.getUserConnectionTime({
            dates: [dates[0], dates[0]],
            userId,
            signal,
          });
        } else if (dates.length == 2) {
          data = await this.getUserConnectionTime({
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

  private getFromTime(info: ConnectionInfo) {
    return moment(info.dateTime).subtract(info.time, "seconds").toDate();
  }
}
</script>