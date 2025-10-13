import axiosClient from "./axiosClient";

// ---- DTO ----
export interface ProfileContactDto {
  id: number;
  contactTypeId: number;
  contactTypeName?: string;
  value: string;
  isActive: boolean;
}

export interface ProfileAddressDto {
  id: number;
  addressTypeId: number;
  addessTypeName?: string;
  addressLine: string;
  city: string;
  zipCode: string;
  countryId: number;
  countryName?: string;
  isActive: boolean;
}

export interface ProfileSettingsDto {
  userId: string;
  languageId?: number;
  languageName: string;
  timezoneId?: number;
  timezoneName: string;
  notificationsEnabled: boolean;
}

export interface ProfileDto {
  userId: string;
  firstName: string;
  lastName: string;
  fullName?: string;
  avatarUrl?: string;
  email?: string;
  birthday?: string;
  createdAt?: string;
  updatedAt?: string;
  contacts: ProfileContactDto[];
  addresses: ProfileAddressDto[];
  settings: ProfileSettingsDto;
}

// ---- Lookup DTO ----
export interface TimezoneDto {
  id: number;
  code: string;
  name: string;
  utsOffset: string;
  offsetMinutes: number;
}

export interface LanguageDto {
  id: number;
  code: string;
  name: string;
  isActive?: boolean;
}

export interface CountryDto {
  id: number;
  alpha2Code: string;
  alpha3Code: string;
  numericCode: number;
  name: string;
}

export interface AddressTypeDto {
  id: number;
  typeName: string;
  typeDescription: string;
  isActive?: boolean;
}

export interface ContactTypeDto {
  id: number;
  typeName: string;
  typeDescription: string;
  isActive?: boolean;
  code: string;
  regexPattern: string;
}

// ---- Pagination ----
export interface PaginationRequest {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProfileSummaryDto {
  userId: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
}

// ---- Service ----
export const profileService = {
  // ---------- PROFILES ----------
  getProfileById: async (userId: string): Promise<ProfileDto> => {
    const response = await axiosClient.get<ProfileDto>(`/profiles/${userId}`);
    return response.data;
  },

  getMyProfile: async (): Promise<ProfileDto> => {
    console.log("try my profile");
    const response = await axiosClient.get<ProfileDto>("/profiles/me");
    return response.data;
  },

  getProfiles: async (
    params: PaginationRequest
  ): Promise<PaginatedResponse<ProfileSummaryDto>> => {
    const response = await axiosClient.get<
      PaginatedResponse<ProfileSummaryDto>
    >("/profiles", { params });
    return response.data;
  },

  updateProfile: async (profile: ProfileDto): Promise<ProfileDto> => {
    const response = await axiosClient.put("/profiles", profile);
    return response.data;
  },

  // ---------- CONTACTS ----------
  getContacts: async (userId: string): Promise<ProfileContactDto[]> => {
    const response = await axiosClient.get<ProfileContactDto[]>(
      `/profiles/${userId}/contacts`
    );
    return response.data;
  },

  getMyContacts: async (): Promise<ProfileContactDto[]> => {
    const response = await axiosClient.get<ProfileContactDto[]>(
      "/profiles/me/contacts"
    );
    return response.data;
  },

  addOrUpdateContact: async (
    userId: string,
    contact: ProfileContactDto
  ): Promise<ProfileContactDto> => {
    const response = await axiosClient.put<ProfileContactDto>(
      `/profiles/${userId}/contacts`,
      contact
    );
    return response.data;
  },

  addOrUpdateMyContact: async (
    contact: ProfileContactDto
  ): Promise<ProfileContactDto> => {
    const response = await axiosClient.put<ProfileContactDto>(
      "/profiles/me/contacts",
      contact
    );
    return response.data;
  },

  deleteMyContact: async (contactId: number): Promise<ProfileContactDto> => {
    const response = await axiosClient.delete(
      `/profiles/me/contacts/${contactId}`
    );
    return response.data;
  },

  // ---------- ADDRESSES ----------
  getAddresses: async (userId: string): Promise<ProfileAddressDto[]> => {
    const response = await axiosClient.get<ProfileAddressDto[]>(
      `/profiles/${userId}/addresses`
    );
    return response.data;
  },

  getMyAddresses: async (): Promise<ProfileAddressDto[]> => {
    const response = await axiosClient.get<ProfileAddressDto[]>(
      "/profiles/me/addresses"
    );
    return response.data;
  },

  addOrUpdateAddress: async (
    userId: string,
    address: ProfileAddressDto
  ): Promise<ProfileAddressDto> => {
    const response = await axiosClient.put<ProfileAddressDto>(
      `/profiles/${userId}/addresses`,
      address
    );
    return response.data;
  },

  addOrUpdateMyAddress: async (
    address: ProfileAddressDto
  ): Promise<ProfileAddressDto> => {
    const response = await axiosClient.put<ProfileAddressDto>(
      "/profiles/me/addresses",
      address
    );
    return response.data;
  },

  deleteMyAddress: async (addressId: number): Promise<ProfileAddressDto> => {
    const response = await axiosClient.delete(
      `/profiles/me/addresses/${addressId}`
    );
    return response.data;
  },

  // ---------- LOOKUPS ----------
  getLanguages: async (): Promise<LanguageDto[]> => {
    const response = await axiosClient.get<LanguageDto[]>(
      "/profiles/languages"
    );
    return response.data;
  },

  getTimezones: async (): Promise<TimezoneDto[]> => {
    const response = await axiosClient.get<TimezoneDto[]>(
      "/profiles/timezones"
    );
    return response.data;
  },

  getCountries: async (): Promise<CountryDto[]> => {
    const response = await axiosClient.get<CountryDto[]>("/profiles/countries");
    return response.data;
  },

  getAddressTypes: async (): Promise<AddressTypeDto[]> => {
    const response = await axiosClient.get<AddressTypeDto[]>(
      "/profiles/addresstypes"
    );
    return response.data;
  },

  getContactTypes: async (): Promise<ContactTypeDto[]> => {
    const response = await axiosClient.get<ContactTypeDto[]>(
      "/profiles/contacttypes"
    );
    return response.data;
  },
};
