import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ImageUrlService {
  
  /**
   * Returns the full image URL.
   * If imagePath is already a full URL (starts with http:// or https://), returns it as-is.
   * Otherwise, prepends the serverUrl to create a relative URL.
   */
  getImageUrl(imagePath: string | null | undefined, serverUrl?: string): string {
    const trimmedPath = imagePath?.trim();
    if (!trimmedPath) {
      return '';
    }

    if (this.isAbsoluteUrl(trimmedPath)) {
      return trimmedPath;
    }

    const normalizedPath = this.normalizePath(trimmedPath);

    if (this.isResourcesPath(normalizedPath)) {
      const resourcesBase = this.normalizeBaseUrl(serverUrl || environment.resourcesUrl || '');
      return `${resourcesBase}${normalizedPath}`;
    }

    const blobBase = this.normalizeBaseUrl(environment.blobBaseUrl || '');
    if (blobBase) {
      return `${blobBase}${normalizedPath}`;
    }

    const fallbackBase = this.normalizeBaseUrl(serverUrl || environment.resourcesUrl || '');
    return `${fallbackBase}${normalizedPath}`;
  }

  private isAbsoluteUrl(path: string): boolean {
    return /^https?:\/\//i.test(path);
  }

  private normalizeBaseUrl(baseUrl: string): string {
    if (!baseUrl) {
      return '';
    }
    return baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
  }

  private normalizePath(path: string): string {
    return path.replace(/\\/g, '/').replace(/^\/+/, '');
  }

  private isResourcesPath(path: string): boolean {
    return path.toLowerCase().startsWith('resources/');
  }
}


