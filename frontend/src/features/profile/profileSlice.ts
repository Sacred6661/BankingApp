import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { profileService } from "../../api/profileService";
import type {
  ProfileDto,
  PaginatedResponse,
  ProfileSummaryDto,
  PaginationRequest,
  ProfileContactDto,
  ProfileAddressDto,
  ProfileSettingsDto,
} from "../../api/profileService";
import { getErrorMessage } from "../../utils/errorHelpers";

// ---- STATE ----
interface ProfileState {
  profile: ProfileDto | null;
  profiles: PaginatedResponse<ProfileSummaryDto> | null;
  loading: boolean;
  error: string | null;
}

const initialState: ProfileState = {
  profile: null,
  profiles: null,
  loading: false,
  error: null,
};

// ---- THUNKS ----

// PROFILE
export const fetchMyProfile = createAsyncThunk<ProfileDto>(
  "profile/fetchMyProfile",
  async (_, thunkAPI) => {
    try {
      return await profileService.getMyProfile();
    } catch (err: any) {
      return thunkAPI.rejectWithValue({
        detail: getErrorMessage(err),
      });
    }
  }
);

export const fetchProfileById = createAsyncThunk<ProfileDto, string>(
  "profile/fetchProfileById",
  async (userId, thunkAPI) => {
    try {
      return await profileService.getProfileById(userId);
    } catch (err: any) {
      return thunkAPI.rejectWithValue({
        detail: getErrorMessage(err),
      });
    }
  }
);

export const fetchProfiles = createAsyncThunk<
  PaginatedResponse<ProfileSummaryDto>,
  PaginationRequest
>("profile/fetchProfiles", async (params, thunkAPI) => {
  try {
    return await profileService.getProfiles(params);
  } catch (err: any) {
    return thunkAPI.rejectWithValue({
      detail: getErrorMessage(err),
    });
  }
});

export const updateProfile = createAsyncThunk<ProfileDto, ProfileDto>(
  "profile/updateProfile",
  async (profile, thunkAPI) => {
    try {
      const updated = await profileService.updateProfile(profile);
      return updated; // ✅ ось що очікує TypeScript
    } catch (err: any) {
      return thunkAPI.rejectWithValue({
        detail: getErrorMessage(err),
      });
    }
  }
);

// CONTACTS
export const addOrUpdateMyContact = createAsyncThunk<
  ProfileContactDto,
  ProfileContactDto
>("profile/addOrUpdateMyContact", async (contact, thunkAPI) => {
  try {
    return await profileService.addOrUpdateMyContact(contact);
  } catch (err: any) {
    return thunkAPI.rejectWithValue({
      detail: getErrorMessage(err),
    });
  }
});

export const deleteMyContact = createAsyncThunk<ProfileContactDto, number>(
  "profile/deleteMyContact",
  async (contactId, thunkAPI) => {
    try {
      return await profileService.deleteMyContact(contactId);
    } catch (err: any) {
      return thunkAPI.rejectWithValue({
        detail: getErrorMessage(err),
      });
    }
  }
);

// ADDRESSES
export const addOrUpdateMyAddress = createAsyncThunk<
  ProfileAddressDto,
  ProfileAddressDto
>("profile/addOrUpdateMyAddress", async (address, thunkAPI) => {
  try {
    return await profileService.addOrUpdateMyAddress(address);
  } catch (err: any) {
    return thunkAPI.rejectWithValue({
      detail: getErrorMessage(err),
    });
  }
});

export const deleteMyAddress = createAsyncThunk<ProfileAddressDto, number>(
  "profile/deleteMyAddress",
  async (addressId, thunkAPI) => {
    try {
      return profileService.deleteMyAddress(addressId);
    } catch (err: any) {
      return thunkAPI.rejectWithValue({
        detail: getErrorMessage(err),
      });
    }
  }
);

// SETTINGS
export const updateProfileSettings = createAsyncThunk<
  ProfileSettingsDto,
  ProfileSettingsDto
>("profile/updateProfileSettings", async (settings, thunkAPI) => {
  try {
    await profileService.updateProfile({
      ...(await profileService.getMyProfile()),
      settings,
    });
    return settings;
  } catch (err: any) {
    return thunkAPI.rejectWithValue({
      detail: getErrorMessage(err),
    });
  }
});

// ---- SLICE ----
const profileSlice = createSlice({
  name: "profile",
  initialState,
  reducers: {
    clearProfile: (state) => {
      state.profile = null;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // fetchMyProfile
      .addCase(fetchMyProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchMyProfile.fulfilled, (state, action) => {
        state.loading = false;
        state.profile = action.payload;
      })
      .addCase(fetchMyProfile.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // fetchProfileById
      .addCase(fetchProfileById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProfileById.fulfilled, (state, action) => {
        state.loading = false;
        state.profile = action.payload;
      })
      .addCase(fetchProfileById.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // fetchProfiles
      .addCase(fetchProfiles.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProfiles.fulfilled, (state, action) => {
        state.loading = false;
        state.profiles = action.payload;
      })
      .addCase(fetchProfiles.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // updateProfile
      .addCase(updateProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateProfile.fulfilled, (state, action) => {
        state.loading = false;
        state.profile = action.payload;
      })
      .addCase(updateProfile.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // addOrUpdateMyContact
      .addCase(addOrUpdateMyContact.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(addOrUpdateMyContact.fulfilled, (state, action) => {
        state.loading = false;
        if (state.profile) {
          const idx = state.profile.contacts.findIndex(
            (c) => c.id === action.payload.id
          );
          if (idx > -1) {
            state.profile.contacts[idx] = action.payload;
          } else {
            state.profile.contacts.push(action.payload);
          }
        }
      })
      .addCase(addOrUpdateMyContact.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // deleteMyContact
      .addCase(deleteMyContact.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteMyContact.fulfilled, (state, action) => {
        state.loading = false;
        if (state.profile) {
          const idx = state.profile.contacts.findIndex(
            (c) => c.id === action.payload.id
          );
          if (idx > -1) {
            state.profile.contacts[idx] = action.payload;
          }
        }
      })
      .addCase(deleteMyContact.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // addOrUpdateMyAddress
      .addCase(addOrUpdateMyAddress.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(addOrUpdateMyAddress.fulfilled, (state, action) => {
        state.loading = false;
        if (state.profile) {
          const idx = state.profile.addresses.findIndex(
            (a) => a.id === action.payload.id
          );
          if (idx > -1) {
            state.profile.addresses[idx] = action.payload;
          } else {
            state.profile.addresses.push(action.payload);
          }
        }
      })
      .addCase(addOrUpdateMyAddress.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // deleteMyAddress
      .addCase(deleteMyAddress.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteMyAddress.fulfilled, (state, action) => {
        state.loading = false;
        if (state.profile) {
          const idx = state.profile.addresses.findIndex(
            (a) => a.id === action.payload.id
          );
          if (idx > -1) {
            state.profile.addresses[idx] = action.payload;
          }
        }
      })
      .addCase(deleteMyAddress.rejected, (state, action: any) => {
        state.loading = false;
        state.error = action.payload;
      })

      // updateProfileSettings
      .addCase(updateProfileSettings.fulfilled, (state, action) => {
        if (state.profile) {
          state.profile.settings = action.payload;
        }
      });
  },
});

export const { clearProfile } = profileSlice.actions;
export default profileSlice.reducer;
