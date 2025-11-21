export interface PrintHeaderSettings {
  customText?: string | null;
  visibility: PrintHeaderVisibility;
}

export interface PrintHeaderVisibility {
  showCompanyName: boolean;
  showCompanyLogo: boolean;
  showCompanyAddress: boolean;
  showCompanyPhone: boolean;
  showCompanyFax: boolean;
  showCompanyEmail: boolean;
  showRegistrationNumber: boolean;
  showCapital: boolean;
  showTaxNumber: boolean;
  showSlogan: boolean;
  showDocumentTitle: boolean;
}

export interface PrintFooterSettings {
  customText?: string | null;
  customTerms?: string | null;
  customNotes?: string | null;
  visibility: PrintFooterVisibility;
}

export interface PrintFooterVisibility {
  showTerms: boolean;
  showNotes: boolean;
  showCustomerSignature: boolean;
  showAuthorizedSignature: boolean;
  showCompanyFooterNote: boolean;
  showDocumentGenerationDate: boolean;
}

// Default values
export const defaultPrintHeaderVisibility: PrintHeaderVisibility = {
  showCompanyName: true,
  showCompanyLogo: true,
  showCompanyAddress: true,
  showCompanyPhone: true,
  showCompanyFax: true,
  showCompanyEmail: true,
  showRegistrationNumber: true,
  showCapital: true,
  showTaxNumber: true,
  showSlogan: true,
  showDocumentTitle: true
};

export const defaultPrintFooterVisibility: PrintFooterVisibility = {
  showTerms: true,
  showNotes: true,
  showCustomerSignature: true,
  showAuthorizedSignature: true,
  showCompanyFooterNote: true,
  showDocumentGenerationDate: true
};






