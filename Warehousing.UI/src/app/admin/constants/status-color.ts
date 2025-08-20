import { Statuses } from "./enums/statuses.enum";

export const STATUS_COLORS: Record<Statuses, string> = {
  [Statuses.PENDING]: '#FFA726',
  [Statuses.PROCESSING]: '#FDD835',
  [Statuses.CONFIRMED]: '#42A5F5',
  [Statuses.SHIPPED]: '#26A69A',
  [Statuses.DELIVERED]: '#66BB6A',
  [Statuses.CANCELLED]: '#ff0000ff',
  [Statuses.RETURNED]: '#AB47BC',
  [Statuses.COMPLETED]: '#3949AB',
  [Statuses.ONHOLD]: '#FFB300',
  [Statuses.FAILED]: '#EF5350',
  [Statuses.DRAFT]: '#686867ff',
};