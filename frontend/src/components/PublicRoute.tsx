import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import LoadingOverlay from "./ui/LoadingOverlay";

export function PublicRoute() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <LoadingOverlay loading={true} />;
  }

  return !isAuthenticated ? <Outlet /> : <Navigate to="/" replace />;
}
