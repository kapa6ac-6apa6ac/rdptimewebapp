import moment from "moment";
import Vue from "vue";

export function HMS(time: number | undefined, humanize = false): string {
  if (time == undefined)
    return "";

  if (humanize)
    if (time == 0)
      return "-";
    else
      return moment.duration(time, "seconds").humanize();

  const h = Math.floor(time / 3600);
  const m = Math.floor((time - h * 3600) / 60);
  const s = Math.floor(time % 60);

  return `${pz(h)}:${pz(m)}:${pz(s)}`;
}

function pz(n: number, z: number | undefined = undefined, s: string | undefined = undefined) {
  z = z || 2, s = s || '0';
  return (n + '').length <= z ? (['', '-'])[+(n < 0)] + (s.repeat(z) + Math.abs(n)).slice(-1 * z) : n + '';
}

Vue.filter("hms", HMS);