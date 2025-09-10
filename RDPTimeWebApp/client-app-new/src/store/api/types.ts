export interface ApiState {
  version: string;
  users: UserInfo[];
  daysInfo: DayInfo[];
}

export interface ConnectionInfo {
  dateTime: Date;
  computer: string;
  time: number;
  ipAddress: string;
}

export interface ScudInfo {
  time: Date;
  doorId: number;
  city: number;
  type: number;
  event: number;
}

export interface DayTimeInfo {
  timeRdp: number;
  timeScud: number;
  timeScud_r: number;
  scudFrom?: Date;
  scudTo?: Date;
  timeVector: number;
  timeManic: number;

  date?: Date;
  id?: number;
  login?: string;
  name?: string;
}

export const DayStates = {
  0: "Рабочий",
  1: "Сокращенный",
  2: "Выходной"
}

export interface UserInfo {
  id: number;
  login: string;
  name: string;
  scudSlvId: number;
  scudUfaId: number;
}

export enum DayType {
  Working = 0,
  HalfHoliday = 1,
  Holiday = 2
}

export interface DayInfo {
  date: Date,
  name: string,
  type: DayType
}