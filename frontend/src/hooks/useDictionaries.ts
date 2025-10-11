// src/hooks/useDictionaries.ts
import { useEffect, useState } from "react";
import type { AddressTypeDto, ContactTypeDto, LanguageDto, TimezoneDto, CountryDto } from "../api/profileService";
import { profileService } from "../api/profileService"

export const useDictionaries = () => {
  const [languages, setLanguages] = useState<LanguageDto[]>([]);
  const [timezones, setTimezones] = useState<TimezoneDto[]>([]);
  const [countries, setCountries] = useState<CountryDto[]>([]);
  const [addressTypes, setAddressTypes] = useState<AddressTypeDto[]>([]);
  const [contactTypes, setContactTypes] = useState<ContactTypeDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const [langs, tzs, cs, ats, cts] = await Promise.all([
          profileService.getLanguages(),
          profileService.getTimezones(),
          profileService.getCountries(),
          profileService.getAddressTypes(),
          profileService.getContactTypes(),
        ]);
        setLanguages(langs);
        setTimezones(tzs);
        setCountries(cs);
        setAddressTypes(ats);
        setContactTypes(cts);
      } catch (err: any) {
        setError(err.message || "Failed to load dictionaries");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return { languages, timezones, countries, addressTypes, contactTypes, loading, error };
};
