import { OrderDto } from "./OrderDto";


export interface ApiResponse {
  success: boolean;
  message: string;
  result: OrderDto;
  insufficientItems: string[];
}