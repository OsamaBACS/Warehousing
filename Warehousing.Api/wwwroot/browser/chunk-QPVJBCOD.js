import{Eb as I,Sa as d,Tb as D,Vb as k,a as f,b as R,d as g,i as h,j as F,l as $,s as c,u as s,v as u,yb as m}from"./chunk-CRG46VMK.js";var M=(r,t,e=u(D))=>e.GetProducts();var C=class r{constructor(t){this.http=t}url=m.baseUrl+"/Supplier/";GetSuppliers(){return this.http.get(`${this.url}GetSuppliers`)}GetSupplierById(t){return this.http.get(`${this.url}GetSupplierById?id=${t}`)}SaveSupplier(t){return this.http.post(`${this.url}SaveSupplier`,t)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var K=(r,t,e=u(C))=>e.GetSuppliers();var _=(r,t,e=u(k))=>e.GetStores();var S=class r{constructor(t){this.http=t}url=m.baseUrl+"/Status/";GetStatusList(){return this.http.get(`${this.url}GetStatusList`)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var nt=(r,t,e=u(S))=>e.GetStatusList();var x=class r{constructor(t){this.http=t}url=m.baseUrl+"/Customers/";GetCustomers(){return this.http.get(`${this.url}GetCustomers`)}GetCustomerById(t){return this.http.get(`${this.url}GetCustomerById?id=${t}`)}SaveCustomer(t){return this.http.post(`${this.url}SaveCustomer`,t)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var mt=(r,t,e=u(x))=>e.GetCustomers();var w=class r{constructor(t){this.http=t}url=m.baseUrl+"/Units/";GetUnits(){return this.http.get(`${this.url}GetUnits`)}GetUnitById(t){return this.http.get(`${this.url}GetUnitById?Id=${t}`)}SaveUnit(t){return this.http.post(`${this.url}SaveUnit`,t)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var bt=(r,t,e=u(w))=>e.GetUnits();var L=class r{constructor(t){this.http=t}url=m.baseUrl+"/Order/";GetOrdersPagination(t,e,i){return this.http.get(`${this.url}GetOrdersPagination?pageIndex=${t}&pageSize=${e}&orderTypeId=${i}`)}FilterOrdersPagination(t){return this.http.post(`${this.url}FilterOrdersPagination`,t)}GetOrderById(t){return this.http.get(`${this.url}GetOrderById?id=${t}`)}GetOrdersByUserId(t){return this.http.post(`${this.url}GetOrdersByUserId`,t)}SaveOrder(t){return this.http.post(`${this.url}SaveOrder`,t)}UpdateOrderStatusToPending(t){return this.http.post(`${this.url}UpdateOrderStatusToPending/${t}`,{})}UpdateOrderStatusToComplete(t){return this.http.post(`${this.url}UpdateOrderStatusToComplete/${t}`,{})}UpdateApprovedOrder(t,e){return this.http.post(`${this.url}UpdateApprovedOrder/${t}`,e)}CancelApprovedOrder(t){return this.http.post(`${this.url}CancelApprovedOrder/${t}`,{})}GetProductVariants(t){return this.http.get(`${this.url}GetProductVariants/${t}`)}GetProductModifiers(t){return this.http.get(`${this.url}GetProductModifiers/${t}`)}SaveOrderWithVariantsAndModifiers(t){return this.http.post(`${this.url}SaveOrderWithVariantsAndModifiers`,t)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var j=class r{constructor(t){this.lang=t}printHtml(t,e="Print Document"){let i=this.lang.currentLang==="ar"?"rtl":"ltr",o=window.open("","_blank","width=800,height=600");o&&(o.document.write(`
      <html dir="${i}">
        <head>
          <title>${e}</title>
          <meta charset="UTF-8">
          <style>
            * {
              box-sizing: border-box;
            }
            
            body {
              font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
              padding: 20px;
              direction: ${i};
              text-align: ${i==="rtl"?"right":"left"};
              color: #000;
              background: white;
              margin: 0;
              line-height: 1.6;
            }
            
            .print-header {
              text-align: center;
              margin-bottom: 30px;
              border-bottom: 2px solid #333;
              padding-bottom: 20px;
            }
            
            .print-header h1 {
              font-size: 28px;
              margin: 0 0 10px 0;
              color: #2c3e50;
            }
            
            .print-header h2 {
              font-size: 20px;
              margin: 0;
              color: #7f8c8d;
              font-weight: normal;
            }
            
            .print-info {
              display: flex;
              justify-content: space-between;
              margin-bottom: 30px;
              flex-wrap: wrap;
            }
            
            .print-info-item {
              background: #f8f9fa;
              border: 1px solid #dee2e6;
              border-radius: 8px;
              padding: 15px;
              margin: 5px;
              flex: 1;
              min-width: 200px;
            }
            
            .print-info-item strong {
              color: #495057;
              display: block;
              margin-bottom: 5px;
              font-size: 14px;
            }
            
            .print-info-item span {
              color: #212529;
              font-size: 16px;
            }
            
            table {
              width: 100%;
              border-collapse: collapse;
              margin: 20px 0;
              font-size: 14px;
              box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            }
            
            th {
              background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
              color: white;
              padding: 15px 12px;
              text-align: ${i==="rtl"?"right":"left"};
              font-weight: 600;
              font-size: 14px;
              border: 1px solid #5a6fd8;
            }
            
            td {
              padding: 12px;
              border: 1px solid #dee2e6;
              vertical-align: top;
              background: white;
            }
            
            tr:nth-child(even) td {
              background: #f8f9fa;
            }
            
            tr:hover td {
              background: #e3f2fd;
            }
            
            .text-center {
              text-align: center;
            }
            
            .text-end {
              text-align: ${i==="rtl"?"right":"left"};
            }
            
            .text-start {
              text-align: ${i==="rtl"?"left":"right"};
            }
            
            .print-total {
              background: #e8f5e8;
              border: 2px solid #28a745;
              border-radius: 8px;
              padding: 20px;
              margin: 30px 0;
              text-align: center;
            }
            
            .print-total h3 {
              margin: 0 0 10px 0;
              color: #155724;
              font-size: 18px;
            }
            
            .print-total .amount {
              font-size: 32px;
              font-weight: bold;
              color: #28a745;
            }
            
            .print-footer {
              margin-top: 40px;
              padding-top: 20px;
              border-top: 1px solid #dee2e6;
              text-align: center;
              color: #6c757d;
              font-size: 12px;
            }
            
            .row {
              display: flex;
              flex-wrap: wrap;
              margin: 0 -10px;
            }
            
            .col-12, .col-md-6 {
              padding: 0 10px;
              margin-bottom: 15px;
            }
            
            .col-12 {
              width: 100%;
            }
            
            .col-md-6 {
              width: 50%;
            }
            
            .badge {
              display: inline-block;
              padding: 4px 8px;
              font-size: 12px;
              font-weight: 500;
              line-height: 1;
              text-align: center;
              white-space: nowrap;
              vertical-align: baseline;
              border-radius: 4px;
            }
            
            .badge-success {
              color: #fff;
              background-color: #28a745;
            }
            
            .badge-danger {
              color: #fff;
              background-color: #dc3545;
            }
            
            .badge-warning {
              color: #212529;
              background-color: #ffc107;
            }
            
            .badge-info {
              color: #fff;
              background-color: #17a2b8;
            }
            
            @media print {
              @page {
                size: A4;
                margin: 15mm;
              }

              body {
                -webkit-print-color-adjust: exact;
                color-adjust: exact;
                zoom: 0.95;
              }

              .print-header {
                page-break-after: avoid;
              }

              table {
                page-break-inside: avoid;
                font-size: 12px;
              }

              th, td {
                border-color: #000 !important;
                padding: 8px 6px;
              }
              
              .col-md-6 {
                width: 50%;
                float: left;
              }
              
              .print-total {
                page-break-before: avoid;
              }
              
              .print-footer {
                page-break-before: avoid;
              }
            }
            
            @media screen {
              .print-info {
                display: flex;
              }
            }
            
            @media screen and (max-width: 768px) {
              .print-info {
                flex-direction: column;
              }
              
              .col-md-6 {
                width: 100%;
              }
            }
          </style>
        </head>
        <body onload="window.print();">
          ${t}
        </body>
      </html>
    `),o.document.close())}printWithOptions(t,e={}){let{title:i="Print Document",showPrintDialog:o=!0,printImmediately:l=!0}=e,p=this.lang.currentLang==="ar"?"rtl":"ltr",a=window.open("","_blank","width=800,height=600");a&&(a.document.write(`
      <html dir="${p}">
        <head>
          <title>${i}</title>
          <meta charset="UTF-8">
          <style>
            /* Same styles as above */
            * { box-sizing: border-box; }
            body {
              font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
              padding: 20px;
              direction: ${p};
              text-align: ${p==="rtl"?"right":"left"};
              color: #000;
              background: white;
              margin: 0;
              line-height: 1.6;
            }
            /* ... (same styles as above) ... */
          </style>
        </head>
        <body ${l?'onload="window.print();"':""}>
          ${t}
        </body>
      </html>
    `),a.document.close(),o&&l&&a.focus())}generatePDFReadyHTML(t,e="Document"){let i=this.lang.currentLang==="ar"?"rtl":"ltr";return`
      <!DOCTYPE html>
      <html dir="${i}">
        <head>
          <title>${e}</title>
          <meta charset="UTF-8">
          <style>
            /* PDF-optimized styles */
            body {
              font-family: 'DejaVu Sans', Arial, sans-serif;
              margin: 0;
              padding: 20px;
              color: #000;
              direction: ${i};
            }
            /* ... (same styles as above but optimized for PDF) ... */
          </style>
        </head>
        <body>
          ${t}
        </body>
      </html>
    `}static \u0275fac=function(e){return new(e||r)(s(I))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var v={printerType:"A4",paperFormat:"A4",paperWidth:210,paperHeight:297,margins:{top:"20mm",right:"20mm",bottom:"20mm",left:"20mm"},fontSettings:{fontFamily:"Arial",baseFontSize:12,headerFontSize:16,footerFontSize:10,tableFontSize:11},printInColor:!0,printBackground:!0,orientation:"Portrait",scale:1},B={printerType:"POS",paperFormat:"Thermal",paperWidth:80,paperHeight:0,margins:{top:"5mm",right:"5mm",bottom:"5mm",left:"5mm"},fontSettings:{fontFamily:"Courier",baseFontSize:10,headerFontSize:12,footerFontSize:8,tableFontSize:9},posSettings:{encoding:"UTF-8",copies:1,autoCut:!0,openCashDrawer:!1,printDensity:8,printSpeed:3,useEscPos:!0,connectionType:"USB",connectionString:null},printInColor:!1,printBackground:!1,orientation:"Portrait",scale:1};function G(r){if(!r)return v;try{let t=JSON.parse(r);return R(f(f({},v),t),{margins:f(f({},v.margins),t.margins),fontSettings:f(f({},v.fontSettings),t.fontSettings),posSettings:t.posSettings?f(f({},B.posSettings),t.posSettings):void 0})}catch{return v}}function xt(r){return JSON.stringify(r)}var U=class r{constructor(t){this.http=t}url=m.baseUrl+"/Company/";GetCompaniesPagination(t,e){return this.http.get(`${this.url}GetCompaniesPagination?pageIndex=${t}&pageSize=${e}`)}SearchCompaniesPagination(t,e,i){return this.http.get(`${this.url}SearchCompaniesPagination?pageIndex=${t}&pageSize=${e}&keyword=${i}`)}SaveCompany(t){return this.http.post(`${this.url}SaveCompany`,t)}GetCompanyById(t){return this.http.get(`${this.url}GetCompanyById?Id=${t}`)}GetCompanies(){return this.http.get(`${this.url}GetCompanies`)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var E=class r{constructor(t,e){this.http=t;this.companiesService=e;this.loadPrinterConfiguration()}baseUrl=`${m.baseUrl}/print`;printerConfig=null;loadPrinterConfiguration(){this.companiesService.GetCompanies().subscribe({next:t=>{t?.printerConfiguration&&(this.printerConfig=G(t.printerConfiguration))},error:t=>{}})}getPrinterConfiguration(){return this.printerConfig}isPosPrinter(){return this.printerConfig?.printerType==="POS"||this.printerConfig?.printerType==="Thermal"}generatePDF(t,e,i="document"){let o={htmlContent:t,title:e,type:i};return this.http.post(`${this.baseUrl}/generate-pdf`,o,{responseType:"blob",observe:"response"}).pipe(F(l=>{let p=l.headers.get("content-type")||"",a=l.body;return a.contentType=p,a.isEscPos=p.includes("octet-stream")||p.includes("escpos"),a}),$(l=>{throw l}))}generateOrderPDF(t){return this.http.post(`${this.baseUrl}/generate-order-pdf`,t,{responseType:"blob"})}generateReportPDF(t){return this.http.post(`${this.baseUrl}/generate-report-pdf`,t,{responseType:"blob"})}printPDF(t,e,i="document"){return g(this,null,function*(){try{let o=yield h(this.generatePDF(t,e,i));if(!o)throw new Error("No print output generated");if(o.isEscPos||this.isPosPrinter())yield this.sendEscPosToPrinter(o,e);else{let p=URL.createObjectURL(o),a=window.open(p,"_blank");a&&(a.onload=()=>{a.print()}),setTimeout(()=>{URL.revokeObjectURL(p)},1e4)}}catch(o){throw o}})}sendEscPosToPrinter(t,e){return g(this,null,function*(){try{let i=yield t.arrayBuffer(),o=new Uint8Array(i);if("usb"in navigator)try{let a=yield navigator.usb.requestDevice({filters:[{classCode:7}]});yield a.open(),yield a.selectConfiguration(1),yield a.claimInterface(0),yield a.transferOut(1,o),yield a.close();return}catch{}let l=URL.createObjectURL(t),p=document.createElement("a");p.href=l,p.download=`${e}.escpos`,document.body.appendChild(p),p.click(),document.body.removeChild(p),URL.revokeObjectURL(l),alert("\u062A\u0645 \u062A\u062D\u0645\u064A\u0644 \u0645\u0644\u0641 ESC/POS. \u064A\u0631\u062C\u0649 \u0625\u0631\u0633\u0627\u0644\u0647 \u0625\u0644\u0649 \u0627\u0644\u0637\u0627\u0628\u0639\u0629 \u064A\u062F\u0648\u064A\u0627\u064B \u0623\u0648 \u0627\u0633\u062A\u062E\u062F\u0627\u0645 \u0628\u0631\u0646\u0627\u0645\u062C \u0627\u0644\u0637\u0628\u0627\u0639\u0629 \u0627\u0644\u0645\u062E\u0635\u0635.")}catch(i){throw i}})}downloadPDF(t,e,i="document"){return g(this,null,function*(){try{let o=yield h(this.generatePDF(t,e,i));if(!o)throw new Error("No print output generated");let l=o.isEscPos||this.isPosPrinter(),p=l?"escpos":"pdf",a=l?"application/octet-stream":"application/pdf",O=URL.createObjectURL(o),P=document.createElement("a");P.href=O,P.download=`${e}.${p}`,document.body.appendChild(P),P.click(),document.body.removeChild(P),URL.revokeObjectURL(O)}catch(o){throw o}})}printOrderPDF(t){return g(this,null,function*(){try{let e=yield h(this.generateOrderPDF(t));if(e){let i=URL.createObjectURL(e),o=window.open(i,"_blank");o&&(o.onload=()=>{o.print()}),setTimeout(()=>{URL.revokeObjectURL(i)},1e4)}}catch(e){throw e}})}downloadOrderPDF(t){return g(this,null,function*(){try{let e=yield h(this.generateOrderPDF(t));if(e){let i=URL.createObjectURL(e),o=document.createElement("a");o.href=i,o.download=`Order-${t.orderId}.pdf`,document.body.appendChild(o),o.click(),document.body.removeChild(o),URL.revokeObjectURL(i)}}catch(e){throw e}})}printReportPDF(t){return g(this,null,function*(){try{let e=yield h(this.generateReportPDF(t));if(e){let i=URL.createObjectURL(e),o=window.open(i,"_blank");o&&(o.onload=()=>{o.print()}),setTimeout(()=>{URL.revokeObjectURL(i)},1e4)}}catch(e){throw e}})}downloadReportPDF(t){return g(this,null,function*(){try{let e=yield h(this.generateReportPDF(t));if(e){let i=URL.createObjectURL(e),o=document.createElement("a");o.href=i,o.download=`${t.reportType}.pdf`,document.body.appendChild(o),o.click(),document.body.removeChild(o),URL.revokeObjectURL(i)}}catch(e){throw e}})}isServiceAvailable(){return g(this,null,function*(){try{return yield h(this.http.get(`${this.baseUrl}/health`)),!0}catch{return!1}})}static \u0275fac=function(e){return new(e||r)(s(d),s(U))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var T=class r{constructor(t){this.http=t}url=m.baseUrl+"/Users/";GetUsersPagination(t,e){return this.http.get(this.url+`GetUsersPagination?pageIndex=${t}&pageSize=${e}`)}SearchUsersPagination(t,e,i){return this.http.get(this.url+`SearchUsersPagination?pageIndex=${t}&pageSize=${e}&keyword=${i}`)}SaveUser(t){return this.http.post(`${this.url}SaveUser`,t)}GetUsers(){return this.http.get(`${this.url}GetUsers`)}GetUserById(t){return this.http.get(`${this.url}GetUserById?id=${t}`)}ChangePasswordForAdmin(t){return this.http.get(`${this.url}ChangePasswordForAdmin?id=${t}`)}static \u0275fac=function(e){return new(e||r)(s(d))};static \u0275prov=c({token:r,factory:r.\u0275fac,providedIn:"root"})};var A=(n=>(n[n.PENDING=0]="PENDING",n[n.PROCESSING=1]="PROCESSING",n[n.CONFIRMED=2]="CONFIRMED",n[n.SHIPPED=3]="SHIPPED",n[n.DELIVERED=4]="DELIVERED",n[n.CANCELLED=5]="CANCELLED",n[n.RETURNED=6]="RETURNED",n[n.COMPLETED=7]="COMPLETED",n[n.ONHOLD=8]="ONHOLD",n[n.FAILED=9]="FAILED",n[n.DRAFT=10]="DRAFT",n))(A||{});var zt={0:"#FFA726",1:"#FDD835",2:"#42A5F5",3:"#26A69A",4:"#66BB6A",5:"#EF5350",6:"#AB47BC",7:"#3949AB",8:"#FFB300",9:"#EF5350",10:"#686867"};export{x as a,C as b,L as c,j as d,v as e,B as f,G as g,xt as h,U as i,E as j,T as k,M as l,K as m,_ as n,S as o,nt as p,mt as q,A as r,zt as s,w as t,bt as u};
