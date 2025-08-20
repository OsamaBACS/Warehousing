import { Role } from "./role";
import { User } from "./users";

export interface UserRole {
    id: number;
    user: User;
    userId: number;
    role: Role;
    roleId: number;
}