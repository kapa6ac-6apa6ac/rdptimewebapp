import moment from "moment";
import Vue from "vue";

export function DateTime(date: string, humanize = false): string {
  if (humanize)
    return moment(date).calendar();

  return moment(date).format("LL LTS");
}

Vue.filter("datetime", DateTime);