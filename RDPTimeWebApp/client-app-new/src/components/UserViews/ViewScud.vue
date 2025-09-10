<template lang="pug">
.card
  b-loading(v-model="isLoading", :is-full-page="false")
  .card-content
    b-table(
      :data="advData",
      default-sort="dateTime",
      :paginated="true",
      :per-page="perPage",
      sticky-header,
      :height="600"
    )
      b-table-column(field="type", label="Тип", v-slot="props")
        b-taglist(attached)
          b-tag(:type="scudData.types[props.row.type].color") {{ scudData.types[props.row.type].name }}
          b-tag(v-if="props.row.tag", type="is-dark") {{ props.row.tag }}
      b-table-column(field="time", label="Время", v-slot="props")
        | {{ props.row.time | datetime }}
      b-table-column(
        field="duration",
        label="Продолжительность",
        v-slot="props"
      )
        | {{ props.row.duration | hms(!fullHms) }}
      b-table-column(
        field="event",
        label="Событие",
        :visible="!showOnlyPass",
        v-slot="props"
      )
        | {{ scudData.events[props.row.event] }}
      b-table-column(field="door", label="Вход", v-slot="props")
        | {{ scudData.cities[props.row.city].doors[props.row.doorId] }}
      b-table-column(field="city", label="Город", v-slot="props")
        | {{ scudData.cities[props.row.city].name }}

      template(#bottom-left)
        b-select(v-model="perPage")
          option(value="15") 15 на странице
          option(value="50") 50 на странице
        b-checkbox.ml-3(v-model="showOnlyPass") Только проход
        b-checkbox.ml-3(v-model="fullHms") Точное время
</template>

<script lang="ts">
import { ScudInfo } from "@/store/api/types";
import { Action } from "vuex-class";
import { Component, Prop, Vue, Watch } from "vue-property-decorator";
import moment from "moment";

import { scudData } from "@/config/scud";

@Component
export default class ViewScud extends Vue {
  private isLoading = true;
  private data: ScudInfo[] = [];
  private abortController!: AbortController;

  private perPage = 15;
  private showOnlyPass = true;
  private scudData = scudData;
  private fullHms = false;

  @Action("getMyScudTime", { namespace: "api" })
  getMyScudTime!: (payload: {
    dates: Date[];
    signal: AbortSignal;
  }) => Promise<ScudInfo[]>;

  @Action("getUserScudTime", { namespace: "api" })
  getUserScudTime!: (payload: {
    dates: Date[];
    userId: number;
    signal: AbortSignal;
  }) => Promise<ScudInfo[]>;

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
    let data: ScudInfo[] = [];
    try {
      if (userId == null) {
        if (dates.length == 1) {
          data = await this.getMyScudTime({
            dates: [dates[0], dates[0]],
            signal,
          });
        } else if (dates.length == 2) {
          data = await this.getMyScudTime({ dates, signal });
        }
      } else {
        if (dates.length == 1) {
          data = await this.getUserScudTime({
            dates: [dates[0], dates[0]],
            userId,
            signal,
          });
        } else if (dates.length == 2) {
          data = await this.getUserScudTime({
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

  private get advData() {
    var data: (ScudInfo & { tag?: string; duration?: number })[] = this.data;

    if (this.showOnlyPass) data = data.filter((d) => d.event == 0);

    let passEvent: number | null = null;
    for (var i = 0; i < data.length; i++) {
      //data[i].duration = "";
      if (data[i].event == 0 && data[i].type == 1) {
        if (passEvent == null) {
          passEvent = i;
        } else {
          data[i].tag = "повторный";
        }
      } else if (data[i].event == 0 && data[i].type == 2) {
        if (passEvent != null) {
          if (
            moment(data[i].time).format("YYYYMMDD") !=
            moment(data[passEvent].time).format("YYYYMMDD")
          ) {
            data[passEvent].tag = "не вышел";
            passEvent = null;
          } else {
            const diff = moment(data[i].time).diff(
              data[passEvent].time,
              "seconds"
            );
            data[i].duration = diff; //moment.duration(diff).humanize();
            passEvent = null;
          }
        } else {
          data[i].tag = "повторный";
        }
      }
    }
    if (passEvent) data[passEvent].tag = "не вышел";

    return data;
  }
}
</script>