import axios from "axios";

const API_URL = "http://localhost:5258/"; // Update to match your backend URL

class AuthService {
  login(username, password) {
    return axios
      .post(API_URL + "login", { username, password })
      .then((response) => {
        if (response.data.token) {
          // Assuming the response includes the user's role
          localStorage.setItem("user", JSON.stringify(response.data));
        }
        return response.data;
      });
  }

  register(username, password, role) {
    return axios.post(API_URL + "register", { username, password, role });
  }

  getStoredUser() {
    return JSON.parse(localStorage.getItem("user"));
  }

  getUserData() {
    const user = JSON.parse(localStorage.getItem("user"));
    if (user && user.token) {
      return axios
        .get(API_URL + "users", {
          headers: { Authorization: `Bearer ${user.token}` },
        })
        .then((response) => {
          return response.data;
        });
    }
  }
}

export default new AuthService();
