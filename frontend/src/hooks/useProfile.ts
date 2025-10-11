import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "./store";
import {
  fetchMyProfile,
  addOrUpdateMyContact,
  addOrUpdateMyAddress,
  updateProfile,
  deleteMyContact,
  deleteMyAddress,
} from "../features/profile/profileSlice";
import type {
  ProfileContactDto,
  ProfileAddressDto,
  ProfileDto,
} from "../api/profileService";

export const useProfile = () => {
  const dispatch = useAppDispatch();
  const profile = useAppSelector((state) => state.profile.profile);
  const loading = useAppSelector((state) => state.profile.loading);
  const error = useAppSelector((state) => state.profile.error);

  useEffect(() => {
    if (!profile) dispatch(fetchMyProfile());
  }, [dispatch, profile]);

  // --- Update Profile ---
  const updateUserProfile = async (profile: ProfileDto) => {
    if (!profile) return Promise.reject(new Error("Profile not loaded"));
    return await dispatch(updateProfile(profile)).unwrap();
  };

  // --- Contacts ---
  const updateContact = async (contact: ProfileContactDto) => {
    if (!profile) return Promise.reject(new Error("Profile not loaded"));
    return await dispatch(addOrUpdateMyContact(contact)).unwrap();
  };

  const deleteContact = async (contactId: number) => {
    if (!profile) return Promise.reject(new Error("Profile not loaded"));
    return await dispatch(deleteMyContact(contactId)).unwrap();
  };

  // --- Addresses ---
  const updateAddress = async (address: ProfileAddressDto) => {
    if (!profile) return Promise.reject(new Error("Profile not loaded"));
    return await dispatch(addOrUpdateMyAddress(address)).unwrap();
  };

  const deleteAddress = async (addressId: number) => {
    if (!profile) return Promise.reject(new Error("Profile not loaded"));
    return await dispatch(deleteMyAddress(addressId)).unwrap();
  };

  return {
    profile,
    loading,
    error,
    addOrUpdateContact: updateContact,
    addOrUpdateAddress: updateAddress,
    updateProfile: updateUserProfile,
    deleteContact,
    deleteAddress,
  };
};
