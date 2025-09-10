import moment from "moment";
import Vue from "vue";

export function Date(date: string, humanize = false): string {
  if (humanize)
    return moment(date).calendar();

  return moment(date).format("ddd, LL");
}

Vue.filter("date", Date);