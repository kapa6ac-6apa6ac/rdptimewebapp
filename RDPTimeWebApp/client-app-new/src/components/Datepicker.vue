<template lang="pug">
b-datepicker(
  v-model="dates",
  range,
  position="is-bottom-left",
  :first-day-of-week="1",
  :unselectable-dates="unselectableDates",
  :events="calendarEvents"
)
  .buttons
    b-button(
      type="is-primary is-light",
      label="Сегодня",
      @click="selectDate('today')"
    )
    b-button(
      type="is-success is-light",
      label="Вчера",
      @click="selectDate('yesterday')"
    )
    b-button(
      type="is-warning is-light",
      label="Эта неделя",
      @click="selectDate('week')"
    )
    b-button(
      type="is-danger is-light",
      label="Прошлая неделя",
      @click="selectDate('last-week')"
    )
    b-button(
      type="is-info is-light",
      label="Этот месяц",
      @click="selectDate('month')"
    )
    b-button(
      type="is-link is-light",
      label="Прошлый месяц",
      @click="selectDate('last-month')"
    )
</template>

<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, VModel, Vue } from "vue-property-decorator";
import moment from "moment";
import { DayInfo, DayType } from "@/store/api/types";

@Component
export default class Datepicker extends Vue {
  @VModel({ default: [] })
  private dates!: Date[];

  @Getter("daysInfo", { namespace: "api" })
  private daysInfo!: DayInfo[];

  @Action("getCalendar", { namespace: "api" })
  getCalendar!: (payload: { from: Date; to: Date }) => Promise<void>;

  async mounted() {
    await this.getCalendar({
      from: moment().subtract(2, "years").toDate(),
      to: moment().add(1, "month").toDate(),
    });
  }

  unselectableDates(day: Date) {
    return day > new Date();
  }

  selectDate(
    type: "today" | "yesterday" | "week" | "last-week" | "month" | "last-month"
  ) {
    if (type == "today") this.dates = [moment().toDate()];
    else if (type == "yesterday")
      this.dates = [moment().subtract(1, "day").toDate()];
    else if (type == "week")
      this.dates = [moment().startOf("week").toDate(), moment().toDate()];
    else if (type == "last-week")
      this.dates = [
        moment().subtract(1, "week").startOf("week").toDate(),
        moment().subtract(1, "week").endOf("week").toDate(),
      ];
    else if (type == "month")
      this.dates = [moment().startOf("month").toDate(), moment().toDate()];
    else if (type == "last-month")
      this.dates = [
        moment().subtract(1, "month").startOf("month").toDate(),
        moment().subtract(1, "month").endOf("month").toDate(),
      ];
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
}
</script>