import moment from "moment";
import { ActionTree } from "vuex";
import { RootState } from "../types";
import { ApiState, ConnectionInfo, DayInfo, DayTimeInfo, ScudInfo, UserInfo } from "./types";

export const actions: ActionTree<ApiState, RootState> = {
  async getMyConnectionTime({ rootGetters }, payload: { dates: Date[], signal: AbortSignal }): Promise<ConnectionInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/me/time/rdp?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      },
      signal: payload.signal
    });
    return await res.json() as ConnectionInfo[];
  },
  async getUserConnectionTime({ rootGetters }, payload: { dates: Date[], userId: number, signal: AbortSignal }): Promise<ConnectionInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user/${payload.userId}/time/rdp?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      },
      signal: payload.signal
    });
    return await res.json() as ConnectionInfo[];
  },
  async getMyScudTime({ rootGetters }, payload: { dates: Date[], signal: AbortSignal }): Promise<ScudInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/me/time/scud?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      },
      signal: payload.signal
    });
    return await res.json() as ScudInfo[];
  },
  async getUserScudTime({ rootGetters }, payload: { dates: Date[], userId: number, signal: AbortSignal }): Promise<ScudInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user/${payload.userId}/time/scud?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      },
      signal: payload.signal
    });
    return await res.json() as ScudInfo[];
  },
  async getMyDayTime({ rootGetters }, payload: { dates: Date[], signal: AbortSignal }): Promise<DayTimeInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/me/time?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      },
      signal: payload.signal
    });
    return await res.json() as DayTimeInfo[];
  },
  async getUserDayTime({ rootGetters }, payload: { dates: Date[], userId: number, signal: AbortSignal }): Promise<DayTimeInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user/${payload.userId}/time?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      },
      signal: payload.signal
    });
    return await res.json() as DayTimeInfo[];
  },
  async getUsersTime({ rootGetters }, payload: { dates: Date[], manicTime: boolean }): Promise<DayTimeInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user/time?from=${payload.dates[0].toUTCString()}&to=${payload.dates[1].toUTCString()}&manicTime=${payload.manicTime}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      }
    });
    return await res.json() as DayTimeInfo[];
  },

  async getUsers({ rootGetters }): Promise<UserInfo[]> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user`, {
      headers: {
        "Authorization": `Bearer ${token}`
      }
    });
    return await res.json() as UserInfo[];
  },
  async loadUsers({ commit, rootGetters }) {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user`, {
      headers: {
        "Authorization": `Bearer ${token}`
      }
    });
    commit('usersLoaded', await res.json());
  },
  async saveUser({ commit, rootGetters }, user: UserInfo) {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/user`, {
      body: JSON.stringify(user),
      method: "PUT",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      }
    });
    if (res.ok) {
      commit("userChanged", user);
    }
  },

  async getCalendar({ commit, rootGetters }, payload: { from: Date, to: Date }): Promise<void> {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/calendar?from=${payload.from.toUTCString()}&to=${payload.to.toUTCString()}`, {
      headers: {
        "Authorization": `Bearer ${token}`
      }
    });
    commit("daysInfoLoaded", await res.json());
    //return await res.json() as DayInfo[];
  },
  async setCalendar({ commit, rootGetters }, day: DayInfo) {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/calendar`, {
      body: JSON.stringify(day),
      method: "POST",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      }
    });
    if (res.ok)
      commit("dayInfoChanged", day);
  },
  async downloadCommonReport({ rootGetters }, payload: { from: Date, to: Date, allUsers: boolean, manicTime: boolean }) {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/v2/report/common?from=${payload.from.toUTCString()}&to=${payload.to.toUTCString()}&allUsers=${payload.allUsers}&manicTime=${payload.manicTime}`, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${token}`,
        "Content-Type": "application/json"
      }
    });
    if (res.ok) {
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = url;
      a.download = moment(payload.from).isSame(payload.to, 'day')
        ? `Отчет за ${moment(payload.from).format('L')}.xlsx`
        : `Отчет за ${moment(payload.from).format('L')} - ${moment(payload.to).format('L')}.xlsx`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      a.remove();
    } else {
      alert("Произошла ошибка при скачивании отчета:\n\n" + res.statusText);
    }
  },
  async syncScudSlv({ rootGetters }) {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/syncScud/salavat`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${token}`,
      }
    });
  },
  async syncScudUfa({ rootGetters }) {
    const token = rootGetters["oidc/oidcAccessToken"];
    const res = await fetch(`/api/syncScud/ufa`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${token}`,
      }
    });
  },
}