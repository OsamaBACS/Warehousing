export interface LoginResult {
    success: boolean;
    token: string | null;
    errorMessage: string | null;
    status: string | null;
}