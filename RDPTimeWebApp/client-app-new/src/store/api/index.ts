import { Module } from "vuex";
import { RootState } from "../types";
import { ApiState } from "./types";
import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: ApiState = {
  version: "0.1",
  users: [],
  daysInfo: []
}

const namespaced = true;

export const api: Module<ApiState, RootState> = {
  namespaced,
  state,
  getters,
  mutations,
  actions
};