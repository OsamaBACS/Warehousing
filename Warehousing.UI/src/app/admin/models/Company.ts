export interface Company {
    id: number;
    nameEn: string;
    nameAr: string;
    addressEn: string;
    addressAr: string;
    phone: string;
    email: string;
    website: string;
    taxNumber: string;
    currencyCode: string;
    footerNoteEn: string;
    footerNoteAr: string;
    termsEn: string;
    termsAr: string;
    logoUrl: string;
    printTemplateId: number | null;
    isActive: boolean;
}

export interface CompanyPagination {
  companies: Company[];
  totals: number;
}
