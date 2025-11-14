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
    fax: string;
    registrationNumber: string;
    capital: number | null;
    sloganEn: string;
    sloganAr: string;
    currencyCode: string;
    footerNoteEn: string;
    footerNoteAr: string;
    termsEn: string;
    termsAr: string;
    logoUrl: string;
    printTemplateId: number | null;
    printerConfiguration?: string | null;
    isActive: boolean;
}

export interface CompanyPagination {
  companies: Company[];
  totals: number;
}
