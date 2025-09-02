import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchMe } from "../features/auth/authSlice";
import type { AppDispatch, RootState } from "../app/store";
import LoadingOverlay from "../components/ui/LoadingOverlay";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const dispatch = useDispatch<AppDispatch>();
  const { loading } = useSelector((state: RootState) => state.auth);
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        await dispatch(fetchMe());
      } catch (err) {
        // 401 and other errors ignore
      } finally {
        if (!cancelled) setInitialized(true); // to set only once
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [dispatch]);

  if (!initialized) {
    return (
      <>
        <LoadingOverlay loading={!initialized || loading} />
        {initialized && children}
      </>
    );
  }

  return <>{children}</>;
}
