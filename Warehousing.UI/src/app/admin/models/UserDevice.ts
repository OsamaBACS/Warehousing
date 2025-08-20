export interface UserDevice {
    id: number;
    userId: number;
    fingerprint: string;
    iPAddress: string;
    isApproved: boolean;
    firstSeen: string;
}

export interface UserDevicePagination {
    devices: UserDevice[];
    totals: number
}