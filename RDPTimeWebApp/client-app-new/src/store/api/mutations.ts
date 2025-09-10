import Vue from "vue";
import { MutationTree } from "vuex";
import { ApiState, DayInfo, UserInfo } from "./types";

export const mutations: MutationTree<ApiState> = {
  usersLoaded(state, payload: UserInfo[]) {
    state.users = payload;
  },
  userChanged(state, user: UserInfo) {
    const i = state.users.findIndex((u) => u.id == user.id);
    if (i > -1)
      Vue.set(state.users, i, user);
  },
  daysInfoLoaded(state, payload: DayInfo[]) {
    state.daysInfo = payload;
  },
  dayInfoChanged(state, day: DayInfo) {
    const i = state.daysInfo.findIndex((d) => d.date == day.date);
    if (i > -1)
      Vue.set(state.daysInfo, i, day);
    else
      state.daysInfo.push(day);
  },
}