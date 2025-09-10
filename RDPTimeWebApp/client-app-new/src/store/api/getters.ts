import { GetterTree } from "vuex";
import { RootState } from "../types";
import { ApiState, DayInfo, UserInfo } from "./types";

export const getters: GetterTree<ApiState, RootState> = {
  users(state): UserInfo[] {
    return state.users;
  },
  daysInfo(state): DayInfo[] {
    return state.daysInfo;
  }
}