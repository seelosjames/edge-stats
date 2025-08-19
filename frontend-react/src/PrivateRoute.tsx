import { useContext } from "react";
import { Navigate, Outlet } from "react-router-dom";
import AuthContext from "./context/AuthContext";

const PrivateRoute = () => {
	const authContext = useContext(AuthContext);
	if (!authContext) return null;
	return authContext.user ? <Outlet /> : <Navigate to="/login" />;
};

export default PrivateRoute;
