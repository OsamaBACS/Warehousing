# 🖨️ Print System Design - Matching Image Template

## Overview
This document outlines the new print customization system designed to match the professional invoice/sales order layout shown in the reference image.

## Database Structure

### 1. Company Entity Enhancements

Added new fields to `Company` entity to support comprehensive print templates:

```csharp
// New fields added:
public string Fax { get; set; } = string.Empty;
public string RegistrationNumber { get; set; } = string.Empty;
public decimal? Capital { get; set; }
public string SloganEn { get; set; } = string.Empty;
public string SloganAr { get; set; } = string.Empty;
```

**Existing fields (already in Company):**
- `NameEn`, `NameAr` - Company name
- `AddressEn`, `AddressAr` - Company address
- `Phone`, `Email`, `Website` - Contact information
- `TaxNumber` - Tax identification number
- `LogoUrl` - Company logo
- `TermsEn`, `TermsAr` - Terms and conditions
- `FooterNoteEn`, `FooterNoteAr` - Footer notes

### 2. User Print Settings (JSON Structure)

The `User.PrintHeader` and `User.PrintFooter` columns will store JSON strings for flexible customization:

**PrintHeader JSON Structure:**
```json
{
  "customText": "Optional custom HTML/text at top",
  "visibility": {
    "showCompanyName": true,
    "showCompanyLogo": true,
    "showCompanyAddress": true,
    "showCompanyPhone": true,
    "showCompanyFax": true,
    "showCompanyEmail": true,
    "showRegistrationNumber": true,
    "showCapital": true,
    "showTaxNumber": true,
    "showSlogan": true,
    "showDocumentTitle": true
  }
}
```

**PrintFooter JSON Structure:**
```json
{
  "customText": "Optional custom HTML/text at bottom",
  "customTerms": "Override company terms if needed",
  "customNotes": "Custom notes/instructions",
  "visibility": {
    "showTerms": true,
    "showNotes": true,
    "showCustomerSignature": true,
    "showAuthorizedSignature": true,
    "showCompanyFooterNote": true,
    "showDocumentGenerationDate": true
  }
}
```

**Backward Compatibility:**
- If `PrintHeader`/`PrintFooter` contains plain text (not JSON), it will be treated as `customText`
- Migration will preserve existing data

## Print Template Layout (Matching Image)

### Header Section:
```
┌─────────────────────────────────────────────────────────────┐
│  [Logo]    Company Name (Large, Centered)                   │
│             Slogan (if enabled)                              │
│  ────────────────────────────────────────────────────────   │
│  Address    │    Phone / Fax                                │
│  Registration: XXXX  │  Capital: XXXX  │  Tax: XXXX         │
└─────────────────────────────────────────────────────────────┘
```

### Document Title:
```
            أمر المبيعات / Sales Order
            ────────────────
```

### Order Information Section:
```
┌─────────────────────────────────────────────────────────────┐
│  Sales Order Code: SO-YYYY-XXXXX                             │
│  Date: DD-MM-YYYY                                            │
│  Customer: [Name]                                            │
│  Description: [Customer Details]                              │
│                                                              │
│  Sales Rep: [Name]                                           │
│  Rep Phone: [Phone]                                          │
│  Inventory/Store: [Store Name]                               │
│  Region: [Region Name]                                       │
└─────────────────────────────────────────────────────────────┘
```

### Product Table:
```
┌───┬──────────┬─────────┬────────┬───────────┬──────┬──────┬──────────┐
│ # │ Product  │ Quantity│ Remaining│ Box Cap │Boxes│ Price │  Total   │
│   │          │         │ Quantity │         │      │       │          │
├───┼──────────┼─────────┼──────────┼──────────┼──────┼──────┼──────────┤
│ 1 │ Product  │  1.000  │  1.000  │  1.00    │  1   │ 90.00│  90.000  │
│   │ Name     │         │          │          │      │      │          │
│   │ [Variant]│         │          │          │      │      │          │
│   │ [Store]  │         │          │          │      │      │          │
│   │ [Notes]  │         │          │          │      │      │          │
└───┴──────────┴─────────┴──────────┴──────────┴──────┴──────┴──────────┘
```

### Summary Section:
```
┌─────────────────────────────────────────────────────────────┐
│  Total Before Discount: 201.000                              │
│  Total Discount: 0.000                                       │
│  Net Total: 201.000                                         │
└─────────────────────────────────────────────────────────────┘
```

### Footer Section:
```
┌─────────────────────────────────────────────────────────────┐
│  Notes: [User-defined notes field]                          │
│                                                              │
│  Terms & Conditions:                                         │
│  - Delivery terms                                           │
│  - Return policy                                            │
│  - Other conditions                                         │
│                                                              │
│  [Customer Signature]      [Authorized Signature]          │
│                                                              │
│  Company Footer Note (if enabled)                           │
│  Generated: [Date/Time]                                     │
└─────────────────────────────────────────────────────────────┘
```

## Implementation Plan

### Phase 1: Database & Backend ✅
- [x] Add new fields to Company entity
- [x] Create PrintSettings models
- [x] Update CompanyDto
- [x] Update CompanyRepo
- [x] Create database migration
- [ ] Apply migration to database

### Phase 2: Frontend Models
- [x] Update Company TypeScript interface
- [ ] Update User interface for PrintSettings
- [ ] Create PrintSettings TypeScript interfaces

### Phase 3: Print Template
- [ ] Create new print template component matching image
- [ ] Implement company header with all fields
- [ ] Implement order information section
- [ ] Implement product table with all columns
- [ ] Implement summary section
- [ ] Implement footer with terms and signatures

### Phase 4: Company Form Updates
- [ ] Add Fax field
- [ ] Add Registration Number field
- [ ] Add Capital field
- [ ] Add Slogan (En/Ar) fields
- [ ] Update form validation

### Phase 5: User Print Settings UI
- [ ] Create print settings component
- [ ] Allow users to customize visibility options
- [ ] Allow users to add custom header/footer text
- [ ] Store settings as JSON

## Data Flow

1. **Company Data** (Shared across all users):
   - Stored in `Company` table
   - Managed by admin through Company form
   - Used as base for all print templates

2. **User Print Settings** (Per-user customization):
   - Stored as JSON in `User.PrintHeader` and `User.PrintFooter`
   - Allows individual users to:
     - Show/hide specific elements
     - Add custom text/html
     - Override terms and conditions
     - Customize footer notes

3. **Order Data** (Dynamic per document):
   - Sales Order Code (generated)
   - Date, Customer/Supplier
   - Sales Representative (from CreatedBy or User)
   - Store/Inventory location
   - Product items with quantities
   - Totals and discounts

## Additional Columns Needed

### For Product Table Enhancement (Future):
- `BoxCapacity` - Capacity per box (may need to be added to Product)
- `RemainingQuantity` - Calculated from inventory
- QR Code generation per item (may require additional service)

### For Sales Representative:
- Order.CreatedBy → User (to get sales rep info)
- Or add explicit SalesRepId to Order entity

## Next Steps

1. Apply the migration to add new Company fields
2. Update company form UI to include new fields
3. Create the print template matching the image design
4. Implement PrintSettings UI for user customization
5. Test print output with all fields populated







