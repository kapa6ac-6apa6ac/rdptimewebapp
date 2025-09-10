import moment from "moment";
import Vue from "vue";

export function Time(date: string, humanize = false): string {
  if (humanize)
    return moment(date).calendar();

  return moment(date).format("LTS");
}

Vue.filter("time", Time);