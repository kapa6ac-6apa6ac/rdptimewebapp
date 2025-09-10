<template lang="pug">
.container.mt-5
  .is-flex
    b-datepicker(
      v-model="day",
      inline,
      :first-day-of-week="1",
      :events="calendarEvents"
    )
    .is-flex-grow-1.card(v-if="dayInfo != null")
      .card-content
        b-field(label="Дата")
          | {{ dayInfo.date | date }}
        b-field(label="Тип")
          b-select(v-model="dayInfo.type")
            option(:value="0") Рабочий день
            option(:value="1") Сокращенный день
            option(:value="2") Выходной
        b-field(label="Название")
          b-input(v-model="dayInfo.name")
        b-button(type="is-success", @click="saveDay") Сохранить
</template>

<script lang="ts">
import { Component, Watch } from "vue-property-decorator";
import Vue from "vue";
import { DayInfo, DayType } from "@/store/api/types";
import { Action, Getter } from "vuex-class";
import moment from "moment";

@Component
export default class Calendar extends Vue {
  private day: Date | null = null;
  private dayInfo: DayInfo | null = null;

  @Getter("daysInfo", { namespace: "api" })
  private daysInfo!: DayInfo[];

  @Action("getCalendar", { namespace: "api" })
  getCalendar!: (payload: { from: Date; to: Date }) => Promise<void>;

  @Action("setCalendar", { namespace: "api" })
  setCalendar!: (payload: DayInfo) => Promise<void>;

  async mounted() {
    await this.getCalendar({
      from: moment().subtract(2, "years").toDate(),
      to: moment().add(1, "month").toDate(),
    });
  }

  async saveDay() {
    if (this.dayInfo != null) {
      await this.setCalendar(this.dayInfo);
    }
  }

  get calendarEvents() {
    const events: { date: Date; type: string }[] = [];
    const to = moment().add(1, "month");
    for (let i = moment().subtract(2, "years"); i <= to; i = i.add(1, "day")) {
      const d = this.daysInfo.find((day) => i.isSame(day.date, "day"));
      if (d != null) {
        if (d.type == DayType.Holiday)
          events.push({
            date: i.toDate(),
            type: "is-danger",
          });
        else if (d.type == DayType.HalfHoliday)
          events.push({
            date: i.toDate(),
            type: "is-warning",
          });
      } else if (i.weekday() > 4) {
        events.push({
          date: i.toDate(),
          type: "is-danger",
        });
      }
    }
    return events;
  }

  @Watch("day")
  private onDayChanged() {
    if (this.day != null) {
      const day = this.daysInfo.find((d) => moment(this.day).isSame(d.date));
      if (day != null) this.dayInfo = day;
      else
        this.dayInfo = {
          date: moment(this.day).utcOffset(0, true).toDate(),
          name: "",
          type:
            moment(this.day).weekday() > 4 ? DayType.Holiday : DayType.Working,
        };
    } else {
      this.dayInfo = null;
    }
  }
}
</script>